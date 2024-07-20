﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrEee.Utility;

namespace FrEee.Ecs.Stats
{
    /// <summary>
    /// A type of stat that some <see cref="IEntity"/> can have.
    /// </summary>
    /// <param name="Name">The name of the stat.</param>
    /// <param name="StackingRule">Rule for stacking stat values.</param>
    public record StatType
    (
        string Name
    )
    {
		public static StatType Unknown => new("Unknown");

		public static StatType WarpDamage => new("Warp Damage");

		public static StatType ColonyResourceExtractionMinerals => new("Colony Resource Extraction Minerals");
		public static StatType ColonyResourceExtractionOrganics => new("Colony Resource Extraction Radioactives");
		public static StatType ColonyResourceExtractionRadioactives => new("Colony Resource Extraction Radioactives");
		public static StatType ColonyResourceExtraction(Resource resource) => ColonyResourceExtraction(resource.Number);
		public static StatType ColonyResourceExtraction(string resourceName) => ColonyResourceExtraction(Resource.Find(resourceName));
		public static StatType ColonyResourceExtraction(int resourceNumber) => resourceNumber switch
		{
			1 => ColonyResourceExtractionMinerals,
			2 => ColonyResourceExtractionOrganics,
			3 => ColonyResourceExtractionRadioactives,
			_ => throw new ArgumentException($"Can't get colony resource extraction for resource number {resourceNumber}. Must be between 1 and 3.")
		};

		public static StatType SpaceYardRateMinerals => new("Space Yard Rate Minerals");
		public static StatType SpaceYardRateOrganics => new("Space Yard Rate Organics");
		public static StatType SpaceYardRateRadioactives => new("Space Yard Rate Radioactives");
		public static StatType SpaceYardRate(Resource resource) => SpaceYardRate(resource.Number);
		public static StatType SpaceYardRate(string resourceName) => SpaceYardRate(Resource.Find(resourceName));
		public static StatType SpaceYardRate(int resourceNumber) => resourceNumber switch
		{
			1 => SpaceYardRateMinerals,
			2 => SpaceYardRateOrganics,
			3 => SpaceYardRateRadioactives,
			_ => throw new ArgumentException($"Can't get space yard rate for resource number {resourceNumber}. Must be between 1 and 3.")
		};

		public static StatType FacilitySize { get; } = new("Facility Size");
	}
}
