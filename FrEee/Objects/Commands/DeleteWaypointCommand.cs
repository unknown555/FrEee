﻿using FrEee.Objects.Civilization;
using FrEee.Extensions;

namespace FrEee.Objects.Commands;

/// <summary>
/// A command to delete a waypoint.
/// </summary>
public class DeleteWaypointCommand : Command<Waypoint>
{
	public DeleteWaypointCommand(Waypoint waypoint)
		: base(waypoint)
	{
	}

	public override void Execute()
	{
		// sanity check
		var emp = Executor.Owner;
		if (emp != Issuer)
		{
			Issuer.Log.Add(Issuer.CreateLogMessage("We cannot issue a command to delete another empire's waypoints!", LogMessages.LogMessageType.Error));
			return;
		}

		Executor.Dispose();

		Issuer.Log.Add(Issuer.CreateLogMessage(Executor.AlteredQueuesOnDelete + " vehicles' orders were truncated when " + Executor + " was deleted.", LogMessages.LogMessageType.Warning));
	}
}