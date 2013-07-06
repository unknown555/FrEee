﻿using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Combat;
using FrEee.Game.Objects.Vehicles;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Civilization
{
	/// <summary>
	/// Cargo stored on a colony or ship/base.
	/// </summary>
	public class Cargo : IDamageable
	{
		public Cargo()
		{
			Population = new SafeDictionary<Race, long>();
			Units = new HashSet<Unit>();
		}

		/// <summary>
		/// The population stored in cargo.
		/// </summary>
		public SafeDictionary<Race, long> Population { get; set; }

		/// <summary>
		/// The units stored in cargo.
		/// </summary>
		public ICollection<Unit> Units { get; set; }

		private int? fakeSize { get; set; }

		/// <summary>
		/// Sets this cargo's fake size to its size and clears the actual population and units.
		/// Used for fog of war.
		/// </summary>
		public void SetFakeSize()
		{
			fakeSize = Size;
			Population.Clear();
			Units.Clear();
		}

		/// <summary>
		/// The amount of space taken by this cargo.
		/// </summary>
		public int Size
		{
			get
			{
				if (fakeSize != null)
					return fakeSize.Value;

				// TODO - moddable population size, perhaps per race?
				return (int)Math.Round(Population.Sum(kvp => kvp.Value) * 5 / 1e6) + Units.Sum(u => u.Design.Hull.Size);
			}
		}

		[DoNotSerialize]
		public int Hitpoints
		{
			get
			{
				// TODO - moddable population HP
				int popHPPerMillion = 100;
				return Population.Sum(kvp => (int)Math.Ceiling(kvp.Value * popHPPerMillion / 1e6)) + Units.Sum(u => u.Hitpoints);
			}
			set
			{
				throw new NotSupportedException("Can't set cargo HP; it's computed.");
			}
		}

		[DoNotSerialize]
		public int NormalShields
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException("Cargo cannot have shields.");
			}
		}

		[DoNotSerialize]
		public int PhasedShields
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException("Cargo cannot have shields.");
			}
		}

		public int MaxHitpoints
		{
			get
			{ 
				// TODO - moddable population HP
				int popHPPerMillion = 100;
				return Population.Sum(kvp => (int)(kvp.Value * popHPPerMillion / (int)1e6)) + Units.Sum(u => u.MaxHitpoints);
			}
		}

		public int MaxNormalShields
		{
			get { return 0; }
		}

		public int MaxPhasedShields
		{
			get { return 0; }
		}

		public void ReplenishShields()
		{
			// do nothing
		}

		public int TakeDamage(DamageType dmgType, int damage, Battle battle)
		{
			if (Population.Any() && Units.Any())
			{
				// for now, have a 50% chance to hit population first and a 50% chance to hit units first
				// TODO - base the chance to hit population vs. units on relative HP or something?
				var coin = RandomHelper.Next(2);
				int leftover;
				if (coin == 0)
					leftover = TakePopulationDamage(dmgType, damage, battle);
				else
					leftover = TakeUnitDamage(dmgType, damage, battle);
				if (coin == 0)
					return TakeUnitDamage(dmgType, leftover, battle);
				else
					return TakePopulationDamage(dmgType, damage, battle);

			}
			else if (Population.Any())
				return TakePopulationDamage(dmgType, damage, battle);
			else if (Units.Any())
				return TakeUnitDamage(dmgType, damage, battle);
			else
				return damage; // nothing to damage
		}

		private int TakePopulationDamage(DamageType dmgType, int damage, Battle battle)
		{
			var killed = new SafeDictionary<Race, int>();
			for (int i = 0; i < damage; i++)
			{
				// pick a race and kill some population
				var race = Population.PickWeighted();
				// TODO - moddable population HP
				int popHPPerMillion = 100;
				int popKilled = (int)1e6 / popHPPerMillion;
				Population[race] -= popKilled;
				killed[race] += popKilled;
			}
			if (battle != null)
			{
				foreach (var race in killed.Keys)
				{
					battle.LogPopulationDamage(race, killed[race]);
				}
			}
			return damage;
		}

		private int TakeUnitDamage(DamageType dmgType, int damage, Battle battle)
		{
			// units with more HP are more likely to get hit first, like with leaky armor
			var units = Units.ToDictionary(u => u, u => u.MaxHitpoints);
			while (units.Any() && damage > 0)
			{
				var u = units.PickWeighted();
				damage = u.TakeDamage(dmgType, damage, battle);
			}
			return damage;
		}

		public bool IsDestroyed
		{
			get { return Hitpoints <= 0; }
		}

		/// <summary>
		/// Passes repair on to units.
		/// Tries to repair more-damaged units first.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public int Repair(int? amount = null)
		{
			if (amount == null)
			{
				foreach (var u in Units.OrderBy(u => (double)u.Hitpoints / (double)u.MaxHitpoints))
					u.Repair(amount);
				return 0;
			}
			else
			{
				foreach (var u in Units.OrderBy(u => (double)u.Hitpoints / (double)u.MaxHitpoints))
					amount = u.Repair(amount);
				return amount.Value;
			}			
		}


		public int HitChance
		{
			get { return 1; }
		}
	}
}
