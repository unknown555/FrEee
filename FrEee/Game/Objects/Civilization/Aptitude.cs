﻿using FrEee.Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Civilization
{
	/// <summary>
	/// A racial aptitude.
	/// </summary>
	[Serializable]
	public class Aptitude : INamed
	{
		// TODO - moddable aptitudes?
		public static IEnumerable<Aptitude> All
		{
			get
			{
				return new Aptitude[]
				{
					PhysicalStrength,
					Intelligence,
					Cunning,
					EnvironmentalResistance,
					Reproduction, 
					Happiness,
					Aggressiveness,
					Defensiveness,
					PoliticalSavvy,
					Mining,
					Farming,
					Refining,
					Construction,
					Repair,
					Maintenance,
				};
			}
		}

		public static readonly Aptitude PhysicalStrength = new Aptitude
		{
			Name = "Physical Strength",
			AbilityName = "Race - Physical Strength",
		};

		public static readonly Aptitude Intelligence = new Aptitude
		{
			Name = "Intelligence",
			AbilityName = "Race Point Generation Modifier - Research",
		};

		public static readonly Aptitude Cunning = new Aptitude
		{
			Name = "Cunning",
			AbilityName = "Race Point Generation Modifier - Intelligence",
		};

		public static readonly Aptitude EnvironmentalResistance = new Aptitude
		{
			Name = "Environmental Resistance",
			AbilityName = "Race - Environmental Resistance",
		};

		public static readonly Aptitude Reproduction = new Aptitude
		{
			Name = "Reproduction",
			AbilityName = "Race - Reproduction",
		};

		public static readonly Aptitude Happiness = new Aptitude
		{
			Name = "Happiness",
			AbilityName = "Race - Happiness",
		};

		public static readonly Aptitude Aggressiveness = new Aptitude
		{
			Name = "Aggressiveness",
			AbilityName = "Race - Combat To Hit Offense Plus",
		};

		public static readonly Aptitude Defensiveness = new Aptitude
		{
			Name = "Defensiveness",
			AbilityName = "Race - Combat To Hit Defense Plus",
		};

		public static readonly Aptitude PoliticalSavvy = new Aptitude
		{
			Name = "Political Savvy",
			AbilityName = "Race - Political Savvy",
		};

		public static readonly Aptitude Mining = new Aptitude
		{
			Name = "Mining Aptitude",
			AbilityName = "Resource Gen Modifier Race - Minerals",
		};

		public static readonly Aptitude Farming = new Aptitude
		{
			Name = "Farming Aptitude",
			AbilityName = "Resource Gen Modifier Race - Organics",
		};

		public static readonly Aptitude Refining = new Aptitude
		{
			Name = "Refining Aptitude",
			AbilityName = "Resource Gen Modifier Race - Radioactives",
		};

		public static readonly Aptitude Construction = new Aptitude
		{
			Name = "Construction Aptitude",
			AbilityName = "Race - Construction Aptitude",
		};

		public static readonly Aptitude Repair = new Aptitude
		{
			Name = "Repair Aptitude",
			AbilityName = "Race - Repair Aptitude",
		};

		public static readonly Aptitude Maintenance = new Aptitude
		{
			Name = "Maintenance Aptitude",
			AbilityName = "Race - Maintenance Aptitude",
		};


		public string Name { get; set; }

		public int MinPercent { get; set; }

		public int MaxPercent { get; set; }

		public int Cost { get; set; }

		public int Threshold { get; set; }

		public int HighCost { get; set; }

		public int LowCost { get; set; }

		public string AbilityName { get; set; }
	}
}
