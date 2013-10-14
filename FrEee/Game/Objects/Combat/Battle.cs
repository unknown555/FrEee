﻿using FrEee.Game.Enumerations;
using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Civilization;
using FrEee.Game.Objects.LogMessages;
using FrEee.Game.Objects.Space;
using FrEee.Game.Objects.Technology;
using FrEee.Modding;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Combat
{
	public class Battle : INamed, IPictorial
	{
		public Battle(Sector location)
		{
			if (location == null)
				throw new Exception("Battles require a sector location.");
			Location = location;
			Log = new List<LogMessage>();
			Empires = Location.SpaceObjects.OfType<ICombatSpaceObject>().Select(sobj => sobj.Owner).Where(emp => emp != null).Distinct().ToArray();
			Combatants = new HashSet<ICombatObject>(Location.SpaceObjects.OfType<ICombatObject>().Where(o => o.Owner != null).Union(Location.SpaceObjects.OfType<Fleet>().SelectMany(f => f.CombatObjects)));
		}

		/// <summary>
		/// The sector in which this battle took place.
		/// </summary>
		public Sector Location { get; private set; }

		/// <summary>
		/// The empires engagaed in battle.
		/// </summary>
		public IEnumerable<Empire> Empires { get; private set; }

		/// <summary>
		/// The combatants in this battle.
		/// </summary>
		public ISet<ICombatObject> Combatants { get; private set; }

		/// <summary>
		/// Battles are named after any stellar objects in their sector; failing that, they are named after the star system and sector coordinates.
		/// </summary>
		public string Name
		{
			get
			{
				if (Location.SpaceObjects.OfType<StellarObject>().Any())
					return "Battle at " + Location.SpaceObjects.OfType<StellarObject>().Largest();
				var coords = Location.Coordinates;
				return "Battle at " + Location.StarSystem + " sector (" + coords.X + ", " + coords.Y + ")";
			}
		}

		/// <summary>
		/// Resolves the battle.
		/// </summary>
		public void Resolve()
		{
			var reloads = new SafeDictionary<Component, double>();
			var seekers = new Dictionary<Seeker, int>();
			for (int i = 0; i < Mod.Current.Settings.SpaceCombatTurns; i++)
			{
				LogRound(i + 1);
				// TODO - real 2D combat mechanics
				foreach (var seeker in seekers.Keys.ToArray())
				{
					seekers[seeker]--;
					if (seekers[seeker] <= 0)
					{
						seekers.Remove(seeker);
						Log.Add(seeker.CreateLogMessage(seeker + " detonates!"));
						seeker.Target.TakeDamage(seeker.WeaponInfo.DamageType, seeker.Damage[1], this);
					}
					else
						Log.Add(seeker.CreateLogMessage(seeker + " moves closer to " + seeker.Target + " (" + seekers[seeker] + " rounds to detonation)"));
				}
				foreach (var launcher in Combatants.ToArray())
				{
					// find launchable units
					var unitsToLaunch = new List<ISpaceVehicle>();
					if (launcher is Planet)
					{
						// planets can launch infinite units per turn
						var p = (Planet)launcher;
						unitsToLaunch.AddRange(p.Cargo.Units.OfType<ISpaceVehicle>());
					}
					else if (launcher is ICargoTransferrer)
					{
						// ships, etc. can launch units based on abilities
						var ct = (ICargoTransferrer)launcher;
						foreach (var vt in Enum.GetValues(typeof(VehicleTypes)).Cast<VehicleTypes>().Distinct())
						{
							var rate = ct.GetAbilityValue("Launch/Recover " + vt.ToSpacedString()).ToInt();
							unitsToLaunch.AddRange(ct.Cargo.Units.Where(u => u.Design.VehicleType == vt).OfType<ISpaceVehicle>().Take(rate));
						}
					}

					// launch them temporarily for combat
					foreach (var unit in unitsToLaunch)
						Combatants.Add(unit);
				}
				foreach (var attacker in Combatants.Shuffle().Where(sobj => sobj.Weapons.Any()).ToArray())
				{
					if (attacker.IsDestroyed)
						continue;

					var defenders = Combatants.Where(sobj => attacker.CanTarget(sobj) && !sobj.IsDestroyed);
					if (!defenders.Any())
						continue; // no one to shoot at
					var defender = defenders.PickRandom();

					LogSalvo(attacker, defender);
					foreach (var weapon in attacker.Weapons.Where(w => w.CanTarget(defender)))
					{
						while (reloads[weapon] <= 0)
						{
							// fire
							var winfo = weapon.Template.ComponentTemplate.WeaponInfo;
							if (winfo is SeekingWeaponInfo)
							{
								// launch a seeker
								var swinfo = (SeekingWeaponInfo)winfo;
								var seeker = new Seeker(attacker.Owner, weapon, defender);
								seekers.Add(seeker, 20 / swinfo.SeekerSpeed);
								LogLaunch(seeker);
							}
							else
							{
								// direct fire
								weapon.Attack(attacker, defender, this); // TODO - range and accuracy and such
							}
							// TODO - mounts that affect reload rate?
							reloads[weapon] += weapon.Template.ComponentTemplate.WeaponInfo.ReloadRate;
						}

						// reload
						reloads[weapon] -= 1;
					}
				}
			}

			// replenish combatants' shields
			foreach (var combatant in Location.SpaceObjects.OfType<ICombatObject>())
				combatant.ReplenishShields();

			// validate fleets
			foreach (var fleet in Location.SpaceObjects.OfType<Fleet>())
				fleet.Validate();
		}

		public IList<LogMessage> Log { get; private set; }

		public void LogRound(int i)
		{
			Log.Add(new GenericLogMessage("Begin round " + i + "!"));
		}

		public void LogSalvo(ICombatObject attacker, ICombatObject defender)
		{
			Log.Add(new PictorialLogMessage<ICombatObject>(attacker + " attacks " + defender + "!", attacker));
		}

		public void LogShot(Component weapon, bool hit)
		{
			Log.Add(new PictorialLogMessage<Component>("Fires " + weapon + " and " + (hit ? "hits" : "misses") + "!", weapon));
		}

		public void LogLaunch(ICombatObject craft)
		{
			Log.Add(new PictorialLogMessage<ICombatObject>("Launches " + craft + "!", craft));
		}

		public void LogShieldDamage(ICombatObject defender, int damage)
		{
			Log.Add(new PictorialLogMessage<ICombatObject>(defender + "'s shields take " + damage + " damage!", defender));
		}

		public void LogComponentDamage(Component component, int damage)
		{
			if (component.IsDestroyed)
				Log.Add(new PictorialLogMessage<Component>(component + " takes " + damage + " damage and is destroyed!", component));
			else
				Log.Add(new PictorialLogMessage<Component>(component + " takes " + damage + " damage!", component));

		}

		public void LogPopulationDamage(Race race, long casualties)
		{
			Log.Add(race.CreateLogMessage(casualties.ToUnitString(true) + " " + race + " are killed!"));
		}

		public void LogSeekerDamage(Seeker seeker, int damage)
		{
			Log.Add(seeker.CreateLogMessage(seeker + " takes " + damage + " damage!"));
		}

		public void LogTargetDeath(ICombatObject defender)
		{
			Log.Add(new PictorialLogMessage<ICombatObject>(defender + " is destroyed!", defender));
		}

		public System.Drawing.Image Icon
		{
			get { return Combatants.OfType<ISpaceObject>().Largest().Icon; }
		}

		public System.Drawing.Image Portrait
		{
			get { return Combatants.OfType<ISpaceObject>().Largest().Portrait; }
		}
	}
}
