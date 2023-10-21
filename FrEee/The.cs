using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrEee.Interfaces;
using FrEee.Modding;
using FrEee.Objects.Civilization;
using FrEee.Objects.Space;
using FrEee.Processes;
using FrEee.Setup;
using FrEee.Utility;

namespace FrEee;

/// <summary>
/// Das Uber-Singleton!!!
/// </summary>
public static class The
{
	/// <summary>
	/// The filename of the mod currently being played.
	/// </summary>
	public static string ModFileName { get; set; }

	/// <summary>
	/// The mod currently being played.
	/// </summary>
	public static Mod Mod { get; set; }

	/// <summary>
	/// The game currently being played.
	/// </summary>
	public static Game Game { get; set; }

	public static GameSetup Setup => Game.Setup;

	// XXX: why is this null when loading a player GAM file? could it be related to the null colony containers?
	public static ReferrableRepository<IReferrable> ReferrableRepository => Game.ReferrableRepository;

	// XXX: Add more properties for sub-repositories so referrables can be referenced directly via them somehow
	// XXX: wait do we only need referrables and references when serializing?! this simplifies so much!!!

	public static Galaxy Galaxy => Game.Galaxy;

	public static Empire? Empire { get => Game.CurrentEmpire; set => Game.CurrentEmpire = value; }

	public static TurnProcessor TurnProcessor => Game.TurnProcessor;

	public static double Timestamp => TurnProcessor.Timestamp;

	public static AbilityManager AbilityManager => Game.AbilityManager;
}