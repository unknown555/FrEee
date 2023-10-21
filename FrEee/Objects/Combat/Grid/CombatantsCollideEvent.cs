﻿using FrEee.Interfaces;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using FrEee.Utility.Serialization;

namespace FrEee.Objects.Combat.Grid
{
	/// <summary>
	/// When combatants collide - seeker detonation or ship ramming
	/// </summary>
	/// <seealso cref="Game.Game.Objects.Combat.Grid.IBattleEvent" />
	public class CombatantsCollideEvent : BattleEvent
	{
		public CombatantsCollideEvent(Battle battle, ICombatant combatant, ICombatant target, IntVector2 location, int combatantDamage, int targetDamage, bool wasCombatantDisarmed, bool wasTargetDisarmed)
			: base(battle, combatant, location, location)
		{
			Target = target;
			CombatantDamage = combatantDamage;
			TargetDamage = targetDamage;
			WasCombatantDisarmed = wasCombatantDisarmed;
			WasTargetDisarmed = wasTargetDisarmed;
		}


		private GameReference<ICombatant> target { get; set; }

		[DoNotSerialize]
		public ICombatant Target
		{
			get => target?.Value ?? Battle?.StartCombatants?[target.ID];
			set => target = value.ReferViaGalaxy();
		}

		public int CombatantDamage { get; set; }
		public int TargetDamage { get; set; }

		public bool WasCombatantDisarmed { get; set; }
		public bool WasTargetDisarmed { get; set; }
	}
}