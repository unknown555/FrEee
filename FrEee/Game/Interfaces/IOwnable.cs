﻿using FrEee.Game.Objects.Civilization;
using FrEee.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Interfaces
{
	/// <summary>
	/// Something which can be owned by an empire.
	/// </summary>
	public interface IOwnable
	{
		[DoNotCopy]
		Empire Owner { get; }
	}

	/// <summary>
	/// Something whose ownership can be changed.
	/// </summary>
	public interface ITransferrable : IOwnable
	{
		new Empire Owner { get; set; }
	}
}
