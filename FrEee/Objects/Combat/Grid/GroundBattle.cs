using FrEee.Interfaces;
using FrEee.Objects.Civilization;
using FrEee.Objects.LogMessages;
using FrEee.Objects.Space;
using FrEee.Objects.Vehicles;
using FrEee.Modding;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrEee.Objects.Combat.Grid
{
	/// <summary>
	/// A battle which takes place on a planet's surface.
	/// </summary>
	/// <seealso cref="Battle" />
	public class GroundBattle : Battle
	{
		public GroundBattle(Planet location)
			: base()
		{
			Planet = location;
			OriginalPlanetOwner = Planet.Owner;
			Sector = location.Sector ?? throw new Exception("Ground battles require a sector location.");

			// TODO - should weapon platforms participate in ground combat like in SE5?
			Empires = Planet.Cargo.Units.OfType<Troop>().Select(t => t.Owner).Distinct();
			var combatants = new HashSet<ICombatant>(Planet.Cargo.Units.OfType<Troop>());
			for (var i = 0; i < Planet.PopulationFill.Value / The.Mod.Settings.PopulationFactor / (The.Mod.Settings.PopulationPerMilitia == 0 ? 20 : The.Mod.Settings.PopulationPerMilitia); i++)
			{
				var militia = Design.MilitiaDesign.Instantiate(The.Game);
				militia.Owner = Planet.Owner;
				combatants.Add(militia);
			}

			Initialize(combatants);
		}

		public override int DamagePercentage => The.Mod.Settings.GroundCombatDamagePercent;

		public Planet Planet { get; private set; }

		public Empire OriginalPlanetOwner { get; private set; }

		public override void Initialize(IEnumerable<ICombatant> combatants)
		{
			base.Initialize(combatants);

			Empires = Planet.Cargo.Units.OfType<Troop>().Select(t => t.Owner).Distinct();

			var moduloID = (int)(Planet.ID % 100000);
			Dice = new PRNG((int)(moduloID / The.Timestamp * 10));
		}

		public override void PlaceCombatants(SafeDictionary<ICombatant, IntVector2> locations)
		{
			// in ground combat, for now everyone is right on top of each other
			foreach (var c in Combatants)
				locations.Add(c, new IntVector2());
		}

		public override int MaxRounds => The.Mod.Settings.GroundCombatTurns;

		public override void ModifyHappiness()
		{
			foreach (var e in Empires)
			{
				switch (this.ResultFor(e))
				{
					case "victory":
						if (OriginalPlanetOwner != e)
							e.TriggerHappinessChange(hm => hm.EnemyPlanetCaptured);
						break;
					case "defeat":
						if (OriginalPlanetOwner == e)
						{
							e.TriggerHappinessChange(hm => hm.OurPlanetCaptured);
							e.TriggerHappinessChange(hm => hm.OurPlanetLost);
							if (Planet.Colony.IsHomeworld)
								e.TriggerHappinessChange(hm => hm.OurHomeworldLost);
						}
						break;
				}

			}
		}

		public override string Name => $"Ground Battle at {Planet}";
	}
}