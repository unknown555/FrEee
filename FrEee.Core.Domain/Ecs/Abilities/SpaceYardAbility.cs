﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrEee.Ecs.Abilities.Utility;
using FrEee.Ecs.Interactions;
using FrEee.Ecs.Stats;
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
    /// Allows an entity to construct ships, bases, and units.
    /// </summary>
    public class SpaceYardAbility(
		IEntity entity,
		AbilityRule rule,
		Formula<string>? description,
		params IFormula[] values
	) : ResourceRateAbility(entity, rule, description, values)
	{
		public SpaceYardAbility(IEntity entity, AbilityRule rule, Formula<string> resource, Formula<int> rate)
			 : this(entity, rule, null, resource, rate)
		{
		}

		public override StatType GetStatType(Resource resource) =>
			StatType.SpaceYardRate(resource);

		public override void Interact(IInteraction interaction)
		{
			base.Interact(interaction);
			// TODO: build interaction
		}
	}
}
