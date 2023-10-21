﻿using FrEee.Interfaces;
using FrEee.Objects.Civilization;
using FrEee.Utility;
using FrEee.Utility.Extensions;

namespace FrEee.Objects.Commands
{
	/// <summary>
	/// Sets the private name for an object.
	/// </summary>
	public class SetPrivateNameCommand : Command<Empire>
	{
		public SetPrivateNameCommand(Empire empire, INameable target, string name)
			: base(empire)
		{
			Target = target;
		}

		/// <summary>
		/// The name to set.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// What are we clearing the name on?
		/// </summary>
		[DoNotSerialize]
		public INameable Target { get { return target.Value; } set { target = value.ReferViaGalaxy(); } }

		private GameReference<INameable> target { get; set; }

		public override void Execute()
		{
			Executor.PrivateNames[target] = Name;
		}
	}
}