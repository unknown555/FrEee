using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrEee.Interfaces;

namespace FrEee.Serialization
{
	/// <summary>
	/// Attribute which causes a property to be converted to a dictionary mapping in-game IDs to something when serializing.
	/// The key type must implement <see cref="IReferrable"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class GameReferenceKeyedDictionaryAttribute : Attribute
	{
	}
}
