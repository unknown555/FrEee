﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrEee.Ecs.Abilities.Utility;
using FrEee.Ecs.Interactions;
using FrEee.Extensions;
using FrEee.Modding;
using FrEee.Objects.LogMessages;
using FrEee.Objects.Space;
using FrEee.Processes.Combat;
using FrEee.Utility;
using Microsoft.Scripting.Utils;

namespace FrEee.Ecs.Abilities
{
    /// <summary>
    /// Damages vehicles that warp through a warp point.
    /// </summary>
	
    public class WarpDamageAbility(
		IEntity entity,
		AbilityRule rule,
		Formula<string>? description,
		params IFormula[] values
	) : StatModifierAbility(entity, rule, description, values)
	{
		public WarpDamageAbility(IEntity entity, AbilityRule rule, Formula<int> damage)
			 : this(entity, rule, null, damage.ToStringFormula())
		{
			StatName = "Warp Damage";
		}

		public Formula<int> Damage { get; private set; }

		public override void Interact(IInteraction interaction)
		{
			base.Interact(interaction);

			if (interaction is WarpInteraction warp)
			{
				var sobj = warp.WarpingVehicle;
				sobj.TakeNormalDamage(Damage);
				if (sobj.IsDestroyed)
				{
					sobj.Owner.Log.Add(sobj.CreateLogMessage(sobj + " was destroyed by turbulence when traversing " + warp.WarpPoint + ".", LogMessageType.Generic));
				}
				else
				{
					sobj.Owner.Log.Add(sobj.CreateLogMessage(sobj + " took " + Damage + " points of damage from turbulence when traversing " + warp.WarpPoint + ".", LogMessageType.Generic));
				}
			}
		}
	}
}
