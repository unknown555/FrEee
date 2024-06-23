using FrEee.Objects.Civilization;
using FrEee.Serialization;
using FrEee.Extensions;
using System;
using FrEee.Objects.Civilization.CargoStorage;
using FrEee.Objects.Space;
using FrEee.Objects.GameState;
using FrEee.Serialization;
using FrEee.Processes.Combat;
using FrEee.Ecs.Abilities.Utility;

namespace FrEee.Objects.Vehicles;

[Serializable]
public class Fighter : SpaceVehicle, IUnit
{
	public override AbilityTargets AbilityTarget
	{
		get { return AbilityTargets.Fighter; }
	}

	public override bool CanWarp
	{
		get { return false; }
	}

	ICargoContainer IContainable<ICargoContainer>.Entity
	{
		get { return CommonExtensions.FindContainer(this); }
	}

	public override bool ParticipatesInGroundCombat
	{
		get { return false; }
	}

	public override IMobileSpaceObject RecycleContainer
	{
		get { return (this as IUnit).Entity as IMobileSpaceObject; }
	}

	public override bool RequiresSpaceYardQueue
	{
		get { return false; }
	}

	public override WeaponTargets WeaponTargetType
	{
		get { return WeaponTargets.Fighter; }
	}

	public override Visibility CheckVisibility(Empire emp)
	{
		var vis = base.CheckVisibility(emp);
		var sobj = Entity as ISpaceObject;
		if (sobj != null && sobj.HasVisibility(emp, Visibility.Scanned))
			vis = Visibility.Scanned;
		return vis;
	}

	public override void Place(ISpaceObject target)
	{
		CommonExtensions.Place(this, target);
	}

	// HACK - until we end our game and this can be purged
	[DoNotSerialize]
	private Cargo Cargo { get; set;}

	public override bool FillsCombatTile => false;
}