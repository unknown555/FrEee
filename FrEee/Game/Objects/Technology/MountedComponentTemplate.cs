﻿using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Abilities;
using FrEee.Game.Objects.Civilization;
using FrEee.Game.Objects.Vehicles;
using FrEee.Modding;
using FrEee.Modding.Interfaces;
using FrEee.Modding.Templates;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Technology
{
	/// <summary>
	/// A combination of component template and mount.
	/// </summary>
	[Serializable]
	public class MountedComponentTemplate : ITemplate<Component>, INamed, IAbilityObject, IPromotable, IContainable<IDesign>, IFormulaHost
	{
		public MountedComponentTemplate(ComponentTemplate ct, Mount mount = null)
		{
			ComponentTemplate = ct;
			Mount = mount;
		}

		/// <summary>
		/// The component template used.
		/// </summary>
		[DoNotSerialize]
		public ComponentTemplate ComponentTemplate { get { return componentTemplate; } set { componentTemplate = value; } }

		private Reference<ComponentTemplate> componentTemplate { get; set; }

		/// <summary>
		/// The mount used.
		/// </summary>
		[DoNotSerialize]
		public Mount Mount { get { return mount; } set { mount = value; } }

		private Reference<Mount> mount { get; set; }

		/// <summary>
		/// The name of the component, prefixed with the short name of the mount (if any).
		/// </summary>
		public string Name
		{
			get
			{
				if (Mount == null)
					return ComponentTemplate.Name;
				return Mount.ShortName + " " + ComponentTemplate.Name.Evaluate(this);
			}
		}

		[DoNotSerialize]
		public Image Icon
		{
			get
			{
				var icon = (Image)ComponentTemplate.Icon.Clone();
				var g = Graphics.FromImage(icon);
				var font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular);
				var brush = Brushes.White;
				var sf = new StringFormat();
				sf.Alignment = StringAlignment.Near;
				sf.LineAlignment = StringAlignment.Far;
				if (Mount != null)
					g.DrawString(Mount.Code, font, brush, new Point(0, 32), sf);
				return icon;
			}
		}

		[DoNotSerialize]
		public Image Portrait
		{
			get
			{
				return ComponentTemplate.Portrait;
			}
		}

		public Component Instantiate()
		{
			return new Component(this);
		}

		public IEnumerable<Ability> Abilities
		{
			get
			{
				return ComponentTemplate.Abilities.Select(a =>
					{
						var result = new Ability(this)
						{
							Name = a.Name,
							Values = new List<Formula<string>>(a.Values),
							Description = a.Description.Evaluate(this),
						};
						if (Mount != null)
						{
							if (Mount.AbilityPercentages.ContainsKey(a.Name))
							{
								foreach (var p in Mount.AbilityPercentages[a.Name])
								{
									result.Values[p.Key] = (double.Parse(result.Values[p.Key].Evaluate(this)) * p.Value / 100).ToString();
									a.Description = null; // values have been modified, need to use generic description
								}
							}
							if (Mount.AbilityModifiers.ContainsKey(a.Name))
							{
								foreach (var m in Mount.AbilityModifiers[a.Name])
								{
									result.Values[m.Key] = (double.Parse(result.Values[m.Key].Evaluate(this)) + m.Value).ToString();
									a.Description = null; // values have been modified, need to use generic description
								}
							}
						}
						return result;
					});
			}
		}

		public IEnumerable<Ability> UnstackedAbilities
		{
			get { return Abilities; }
		}

		public int Size
		{
			get
			{
				if (Mount == null)
					return ComponentTemplate.Size.Evaluate(this);
				return ComponentTemplate.Size.Evaluate(this) * Mount.SizePercent.Evaluate(this) / 100;
			}
		}

		public ResourceQuantity Cost
		{
			get
			{
				if (Mount == null)
					return ComponentTemplate.Cost.Evaluate(this);
				return ComponentTemplate.Cost.Evaluate(this) * Mount.CostPercent.Evaluate(this) / 100;
			}
		}

		public int SupplyUsage
		{
			get
			{
				if (Mount == null)
					return ComponentTemplate.SupplyUsage.Evaluate(this);
				return ComponentTemplate.SupplyUsage.Evaluate(this) * Mount.SupplyUsagePercent.Evaluate(this) / 100;
			}
		}

		public int Durability
		{
			get
			{
				if (Mount == null)
					return ComponentTemplate.Durability.Evaluate(this);
				return ComponentTemplate.Durability.Evaluate(this) * Mount.DurabilityPercent.Evaluate(this) / 100;
			}
		}

		public static bool operator ==(MountedComponentTemplate t1, MountedComponentTemplate t2)
		{
			if (t1.IsNull() && t2.IsNull())
				return true;
			if (t1.IsNull() || t2.IsNull())
				return false;
			return t1.Container == t2.Container && t1.ComponentTemplate == t2.ComponentTemplate && t1.Mount == t2.Mount;
		}

		public static bool operator !=(MountedComponentTemplate t1, MountedComponentTemplate t2)
		{
			return !(t1 == t2);
		}

		public override bool Equals(object obj)
		{
			if (obj is MountedComponentTemplate)
				return this == (MountedComponentTemplate)obj;
			return false;
		}

		public override int GetHashCode()
		{
			if (Mount == null)
				return Container.GetHashCode() ^ ComponentTemplate.GetHashCode();
			return Container.GetHashCode() ^ ComponentTemplate.GetHashCode() ^ Mount.GetHashCode();
		}

		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// Damage inflicted by this component at range, if it is a weapon.
		/// </summary>
		public int[] WeaponDamage
		{
			get
			{
				var w = ComponentTemplate.WeaponInfo;
				if (w == null)
					return null;
				if (Mount == null)
					return w.Damage;

				var dmg = new List<int>();
				dmg.Add(w.Damage[0] * Mount.WeaponDamagePercent.Evaluate(this) / 100);
				if (Mount.WeaponRangeModifier.Evaluate(this) > 0)
				{
					if (w.Damage.Length > 1)
					{
						// extend range by applying range-1 damage out further
						for (int i = 0; i < Mount.WeaponRangeModifier.Evaluate(this); i++)
							dmg.Add(w.Damage[1] * Mount.WeaponDamagePercent.Evaluate(this) / 100);
					}
					foreach (var d in w.Damage.Skip(1))
						dmg.Add(d * Mount.WeaponDamagePercent.Evaluate(this) / 100);
				}
				else
				{
					// reduce range by applying further-out damage at range 1
					foreach (var d in w.Damage.Skip(-Mount.WeaponRangeModifier.Evaluate(this) + 1))
						dmg.Add(d * Mount.WeaponDamagePercent.Evaluate(this) / 100);
				}
				return dmg.Take(Math.Min(dmg.Count, 21)).ToArray();
			}
		}

		/// <summary>
		/// Accuracy rating of this component, if it is a weapon.
		/// </summary>
		public int WeaponAccuracy
		{
			get
			{
				var w = ComponentTemplate.WeaponInfo;
				if (w == null)
					return 0;
				if (w is DirectFireWeaponInfo)
					return ((DirectFireWeaponInfo)w).AccuracyModifier.Evaluate(this) + (Mount == null ? 0 : Mount.WeaponAccuracyModifier.Evaluate(this));
				return 999; // seekers/warheads
			}
		}

		public void ReplaceClientIDs(IDictionary<long, long> idmap)
		{
			container.ReplaceClientIDs(idmap);
		}


		private Reference<IDesign> container { get; set; }

		/// <summary>
		/// The design which contains this mounted component template.
		/// </summary>
		[DoNotSerialize]
		public IDesign Container { get { return container == null ? null : container.Value; } set { container = value.Reference(); } }

		public IDictionary<string, object> Variables
		{
			get 
			{
				var design = Container ?? Design.Create(Mod.Current.Hulls.FirstOrDefault(h => ComponentTemplate.VehicleTypes.HasFlag(h.VehicleType)));
				var empire = Container == null ? Empire.Current : Container.Owner;
				return new Dictionary<string, object>
				{
					{"component", ComponentTemplate},
					{"mount", Mount},
					{"design", design},
					{"hull", design.Hull},
					{"empire", empire}
				};
			}
		}
	}
}