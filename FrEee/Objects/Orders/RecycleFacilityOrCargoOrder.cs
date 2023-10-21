﻿using FrEee.Interfaces;
using FrEee.Objects.Civilization;
using FrEee.Objects.LogMessages;
using FrEee.Objects.Space;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using FrEee.Utility.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace FrEee.Objects.Orders
{
	public class RecycleFacilityOrCargoOrder : IOrder
	{
		public RecycleFacilityOrCargoOrder(IRecycleBehavior behavior, IRecyclable target)
		{
			Behavior = behavior;
			Target = target;
		}

		public IRecycleBehavior Behavior { get; private set; }

		public bool ConsumesMovement
		{
			get { return false; }
		}

		public long ID
		{
			get;
			set;
		}

		public bool IsComplete
		{
			get;
			set;
		}

		public bool IsDisposed
		{
			get;
			set;
		}

		/// <summary>
		/// The empire which issued the order.
		/// </summary>
		[DoNotSerialize]
		public Empire Owner { get { return owner; } set { owner = value; } }

		/// <summary>
		/// The facility or unit in cargo to recycle.
		/// </summary>
		[DoNotSerialize]
		public IRecyclable Target { get { return target.Value; } set { target = value.ReferViaGalaxy(); } }

		private GameReference<Empire> owner { get; set; }
		private GameReference<IRecyclable> target { get; set; }

		public bool CheckCompletion(IOrderable executor)
		{
			return IsComplete;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			foreach (var v in The.Game.Referrables.OfType<IMobileSpaceObject>())
				v.RemoveOrder(this);
			The.ReferrableRepository.Remove(this);
		}

		public void Execute(IOrderable x)
		{
			if (x is IMobileSpaceObject executor)
			{
				var errors = GetErrors(executor);
				if (errors.Any())
				{
					if (Owner != null)
					{
						foreach (var e in errors)
							Owner.Log.Add(e);
					}
					else
						IsComplete = true;
					return;
				}
			}

			Behavior.Execute(Target);
			IsComplete = true;
		}

		public IEnumerable<LogMessage> GetErrors(IOrderable executor)
		{
			return Behavior.GetErrors(executor as IMobileSpaceObject, Target).Concat(SelfErrors);
		}

		private IEnumerable<LogMessage> SelfErrors
		{
			get
			{
				if (Target is ICombatant c && c.IsHostileTo(Owner))
					yield return c.CreateLogMessage($"You can't {Behavior.Verb} {c} because it belongs to a hostile empire.", LogMessageType.Error);
			}
		}

		public void ReplaceClientIDs(IDictionary<long, long> idmap, ISet<IPromotable> done = null)
		{
			// This type does not use client objects, so nothing to do here.
		}

		public override string ToString()
		{
			return Behavior.Verb + " " + Target;
		}
	}
}