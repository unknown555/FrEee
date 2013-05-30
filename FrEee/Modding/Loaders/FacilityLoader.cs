﻿using FrEee.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrEee.Game.Objects.Technology;

namespace FrEee.Modding.Loaders
{
	/// <summary>
	/// Loads facilities from Facility.txt.
	/// </summary>
	 [Serializable] public class FacilityLoader : ILoader
	{
		public void Load(DataFile df, Mod mod)
		{
			foreach (var rec in df.Records)
			{
				var f = new FacilityTemplate();
				mod.FacilityTemplates.Add(f);

				int index = -1;

				f.Name = rec.GetString("Name", ref index, true, 0, true);
				f.Description = rec.GetString("Description", ref index, true, 0, true);
				f.Group = rec.GetString("Facility Group", ref index, true, 0, true);
				f.Family = rec.GetString("Facility Family", ref index, true, 0, true);
				f.RomanNumeral = rec.GetInt("Roman Numeral", ref index, true, 0, true);
				var picfield = rec.FindField("Pic", ref index, false, 0, true);
				if (picfield != null)
					f.PictureName = picfield.Value;
				else
					f.PictureName = "Facil_" + rec.GetInt("Pic Num", ref index, true, 0, true).ToString("000"); // for compatibility with SE4

				foreach (var costfield in rec.Fields.Where(cf => cf.Name.StartsWith("Cost ")))
					f.Cost[costfield.Name.Substring("Cost ".Length)] = costfield.IntValue(rec);

				foreach (var tr in TechnologyRequirementLoader.Load(rec))
					f.TechnologyRequirements.Add(tr);

				foreach (var abil in AbilityLoader.Load(rec))
					f.Abilities.Add(abil);
			}
		}
	}
}
