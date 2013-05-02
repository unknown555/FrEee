﻿using FrEee.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Modding.Templates
{
	/// <summary>
	/// Chooses a random ability, or none at all, based on a roll of a d1000.
	/// </summary>
	public class RandomAbilityTemplate : ITemplate<Ability>, INamed
	{
		private static Random r = new Random();

		public RandomAbilityTemplate()
		{
			AbilityChances = new List<AbilityChance>();
		}

		/// <summary>
		/// The name of this random ability template.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Chances to get each ability.
		/// Chances are expressed in tenths of a percent.
		/// The total for all chances should add up to 1000 or less.
		/// </summary>
		public IList<AbilityChance> AbilityChances { get; private set; }

		public Ability Instantiate()
		{
			var num = r.Next(1000);
			var howFar = 0;
			foreach (var ac in AbilityChances)
			{
				howFar += ac.Chance;
				if (num < howFar)
					return ac.Ability;
			}
			return null;
		}
	}

	public struct AbilityChance
	{
		/// <summary>
		/// The ability.
		/// </summary>
		public Ability Ability { get; set; }

		/// <summary>
		/// The chance, in tenths of a percent.
		/// </summary>
		public int Chance { get; set; }
	}
}
