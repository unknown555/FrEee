﻿using FrEee.Enumerations;
using FrEee.Interfaces;
using FrEee.Objects.Civilization;
using FrEee.Objects.Space;
using FrEee.Utility;
using System;
using System.Linq;

namespace FrEee.Objects.Vehicles
{
	[Serializable]
	public class Base : MajorSpaceVehicle
	{
		public override AbilityTargets AbilityTarget
		{
			get { return AbilityTargets.Base; }
		}

		public override bool CanWarp
		{
			get { return false; }
		}

		/// <summary>
		/// Bases have infinite supplies.
		/// </summary>
		public override bool HasInfiniteSupplies
		{
			get
			{
				return true;
			}
		}

		public override bool ParticipatesInGroundCombat
		{
			get { return false; }
		}

		public override IMobileSpaceObject RecycleContainer
		{
			get { return this; }
		}

		public override bool RequiresSpaceYardQueue
		{
			get { return true; }
		}
	}
}