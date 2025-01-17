﻿using FrEee.Objects.Civilization;
using FrEee.Objects.Space;
using FrEee.Utility;
using FrEee.Extensions;
using FrEee.UI.WinForms.DataGridView;
using FrEee.UI.WinForms.Interfaces;
using FrEee.UI.WinForms.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FrEee.Objects.Vehicles;
using FrEee.Processes.Combat;
using FrEee.Processes.Combat.Grid;

namespace FrEee.UI.WinForms.Forms;

public partial class BattleResultsForm : GameForm, IBindable<IBattle>
{
	public BattleResultsForm(IBattle battle)
	{
		InitializeComponent();

		Bind(battle);

		try
		{
			this.Icon = new Icon(FrEee.UI.WinForms.Properties.Resources.FrEeeIcon);
		}
		catch { }

		Text = Battle.NameFor(Empire.Current);
	}

	/// <summary>
	/// The battle we are displaying results for.
	/// </summary>
	public IBattle Battle { get; private set; }

	public void Bind(IBattle data)
	{
		Battle = data;
		Bind();
	}

	public void Bind()
	{
		// create grid config
		var cfg = new GridConfig();
		cfg.Name = "Default";
		cfg.Columns.Add(new GridColumnConfig("EmpireIcon", "Flag", typeof(DataGridViewImageColumn), Color.White, Format.Raw));
		cfg.Columns.Add(new GridColumnConfig("EmpireName", "Empire", typeof(DataGridViewTextBoxColumn), Color.White, Format.Raw, Sort.Ascending, 1));
		cfg.Columns.Add(new GridColumnConfig("HullIcon", "Icon", typeof(DataGridViewImageColumn), Color.White, Format.Raw));
		cfg.Columns.Add(new GridColumnConfig("HullName", "Hull", typeof(DataGridViewTextBoxColumn), Color.White, Format.Raw));
		cfg.Columns.Add(new GridColumnConfig("HullSize", "Size", typeof(DataGridViewTextBoxColumn), Color.White, Format.Kilotons, Sort.Descending, 2));
		cfg.Columns.Add(new GridColumnConfig("StartCount", "Start #", typeof(DataGridViewTextBoxColumn), Color.White, Format.UnitsBForBillions));
		cfg.Columns.Add(new GridColumnConfig("StartHP", "Start HP", typeof(DataGridViewTextBoxColumn), Color.White, Format.UnitsBForBillions));
		cfg.Columns.Add(new GridColumnConfig("Losses", "Losses", typeof(DataGridViewTextBoxColumn), Color.White, Format.UnitsBForBillions));
		cfg.Columns.Add(new GridColumnConfig("Damage", "Damage", typeof(DataGridViewTextBoxColumn), Color.White, Format.UnitsBForBillions));
		grid.CurrentGridConfig = cfg;

		// run through sim to get stats
		//Battle.Resolve();

		// gather grid data
		var data = new List<object>();
		var combatants = Battle.StartCombatants;
		foreach (var group in combatants.GroupBy(kvp => new CombatantInfo
		{
			Empire = kvp.Value.Owner,
			HullIcon = GetHullIcon(kvp.Value),
			HullName = GetHullName(kvp.Value),
			HullSize = GetHullSize(kvp.Value)
		}))
		{
			var count = group.Count();
			var hp = group.Sum(c => Battle.StartCombatants[c.Key].Hitpoints);
			var item = new
			{
				EmpireIcon = group.Key?.Empire?.Icon,
				EmpireName = group.Key?.Empire?.Name,
				HullIcon = group.Key.HullIcon,
				HullName = group.Key.HullName,
				HullSize = group.Key.HullSize,
				StartCount = count,
				StartHP = hp,
				Losses = group.Count(kvp => Battle.EndCombatants[kvp.Key].IsDestroyed || Battle.EndCombatants[kvp.Key].Owner != Battle.StartCombatants[kvp.Key].Owner), // destroyed or captured
				Damage = hp - group.Sum(kvp => Battle.EndCombatants[kvp.Key].Hitpoints),
			};
			data.Add(item);
		}
		grid.Data = data;
		grid.Initialize();
	}

	/// <summary>
	/// Gets the icon of a vehicle/design, or a generic icon if it's not a vehicle/design (e.g. if it's a planet).
	/// </summary>
	/// <param name="obj"></param>
	private static Image GetHullIcon(IOwnable obj)
	{
		if (obj is IVehicle)
			return ((IVehicle)obj).Design.Hull.GetIcon(obj.Owner.ShipsetPath).Resize(32);
		if (obj is IDesign)
			return ((IDesign)obj).Hull.GetIcon(obj.Owner.ShipsetPath);
		return Pictures.GetGenericImage(obj.GetType());
	}

	/// <summary>
	/// Gets the hull name of a vehicle/design, or the type name if it's not a vehicle/design (e.g. if it's a planet).
	/// </summary>
	/// <param name="obj"></param>
	private static string GetHullName(IOwnable obj)
	{
		if (obj is IVehicle)
			return ((IVehicle)obj).Design.Hull.Name;
		if (obj is IDesign)
			return ((IDesign)obj).Hull.Name;
		return obj.GetType().Name;
	}

	/// <summary>
	/// Gets the hull size of a vehicle/design, or int.MaxValue if it's not a vehicle/design (e.g. if it's a planet).
	/// </summary>
	/// <param name="obj"></param>
	private static int GetHullSize(IOwnable obj)
	{
		if (obj is IVehicle v)
			return v.Design.Hull.Size;
		if (obj is IDesign d)
			return d.Hull.Size;
		if (obj is Seeker s)
			return s.MaxHitpoints;
		if (obj is Planet p)
			return p.Size.MaxCargo;
		return 42;
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnReplay_Click(object sender, EventArgs e)
	{
		Form form;
		//if (Battle is FrEee.Processes.Combat.Simple.Battle b)
		//	form = new LogForm(MainGameForm.Instance, b.Log);
		if (Battle is Battle b2)
			form = new BattleReplayForm(b2);
		else
			throw new Exception($"Unknown battle type {Battle.GetType()}!");
		this.ShowChildForm(form);
	}

	private class CombatantInfo
	{
		public Empire Empire { get; set; }
		public Image HullIcon { get; set; }
		public string HullName { get; set; }
		public int HullSize { get; set; }

		public override bool Equals(object? obj)
		{
			var ci = obj as CombatantInfo;
			if (ci == null)
				return false;
			return Empire == ci.Empire && HullName == ci.HullName && HullSize == ci.HullSize;
		}

		public override int GetHashCode()
		{
			return HashCodeMasher.Mash(Empire, HullName, HullSize);
		}
	}

	private void btnGoTo_Click(object sender, EventArgs e)
	{
		MainGameForm.Instance.SelectStarSystem(Battle.StarSystem);
		MainGameForm.Instance.SelectSector(Battle.Sector.Coordinates);
		Close();
	}
}