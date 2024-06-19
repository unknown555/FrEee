﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Allows an entity to construct ships, bases, and units.
    /// </summary>
    public class SpaceYardAbility
		: Ability
	{
		public SpaceYardAbility
		(
			IAbilityObject container,
			AbilityRule rule,
			Formula<string>? description,
			params IFormula[] values
		) : base(container, rule, description, values)
		{
			ResourceFormula = values[0].ToStringFormula();
			RateFormula = (Formula<int>)values[1].ToFormula<int>();
		}

		public SpaceYardAbility(IAbilityObject container, AbilityRule rule, Formula<string> resource, Formula<int> rate)
			 : this(container, rule, null, resource, rate)
		{
			ResourceFormula = resource;
			RateFormula = rate;
		}

		public string GetStatName(Resource resource) =>
			$"Space Yard Rate {resource.Name}";

		public Formula<string> ResourceFormula { get; private set; }

		public Formula<int> RateFormula { get; private set; }

		public Resource Resource => Resource.Find(ResourceFormula);

		public int Rate => RateFormula;

		public override void Interact(IInteraction interaction)
		{
			if (interaction is GetStatNamesInteraction getStatNames)
			{
				getStatNames.StatNames.Add(GetStatName(Resource));
			}
			if (interaction is GetStatValueInteraction getStatValue && getStatValue.Stat.Name == GetStatName(Resource))
			{
				getStatValue.Stat.Values.Add(Rate);
			}
		}
	}
}
