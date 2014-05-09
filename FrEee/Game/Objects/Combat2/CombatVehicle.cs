﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrEee.Utility;
using FrEee.Utility.Extensions;

using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Vehicles;
using FrEee.Game.Objects.Combat;
using FrEee.Game.Objects.Technology;
using FrEee.Modding;

using NewtMath.f16;
using FixMath.NET;

namespace FrEee.Game.Objects.Combat2
{
	public class CombatVehicle : CombatControlledObject
	{
        /// <summary>
        /// use this constructor for creating an 'Start' combat Object.
        /// </summary>
        /// <param name="start_v"></param>
        /// <param name="working_v"></param>
        /// <param name="battleseed"></param>
        /// <param name="OrigionalID">this should be the FrEee ID for the origional Vehicle</param>
        /// <param name="IDPrefix"></param>
        public CombatVehicle(Vehicle start_v, Vehicle working_v, int battleseed, long OrigionalID, string IDPrefix = "SHP")
            : base(start_v, working_v, new PointXd(0, 0, 0), new PointXd(0, 0, 0), battleseed, IDPrefix)
        {
            this.ID = OrigionalID;
            // TODO - don't some mods have vehicles >32MT?
            cmbt_mass = (Fix16)working_v.Size;
            maxfowardThrust = (Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1;
            maxStrafeThrust = ((Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1) / ((Fix16)4 - (Fix16)working_v.Evasion * (Fix16)0.01);
            maxRotate.Degrees = ((Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1) / ((Fix16)2.5 - (Fix16)working_v.Evasion * (Fix16)0.01);
            if (start_v.Design.Strategy == null)
                strategy = new StragegyObject_Default();
            else
                strategy = start_v.Design.Strategy;
#if DEBUG
            Console.WriteLine("Created new CombatVehicle with ID " + ID);
            Console.WriteLine("MaxAccel = " + maxfowardThrust / cmbt_mass);
#endif
        }

//        public CombatVehicle(Vehicle start_v, Vehicle working_v, int battleseed, string IDPrefix = "SHP")
//            : base(start_v, working_v, new PointXd(0, 0, 0), new PointXd(0, 0, 0), battleseed, IDPrefix)
//        {
//            // TODO - don't some mods have vehicles >32MT?
//            cmbt_mass = (Fix16)working_v.Size;
//            maxfowardThrust = (Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1;
//            maxStrafeThrust = ((Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1) / ((Fix16)4 - (Fix16)working_v.Evasion * (Fix16)0.01);
//            maxRotate.Radians = ((Fix16)working_v.Speed * this.cmbt_mass * (Fix16)0.1) / ((Fix16)12000 - (Fix16)working_v.Evasion * (Fix16)0.1);
//            if (start_v.Design.Strategy == null)
//                strategy = new StragegyObject_Default();
//            else
//                strategy = start_v.Design.Strategy;
//#if DEBUG
//            Console.WriteLine("MaxAccel = " + maxfowardThrust / cmbt_mass);
//#endif
//        }



		#region fields & properties

		/// <summary>
		/// The vehicle's state at the start of combat.
		/// </summary>
		public Vehicle StartVehicle { get { return (Vehicle)StartCombatant; } private set { StartCombatant = value; } }

		/// <summary>
		/// The current state of the vehicle.
		/// </summary>
		public Vehicle WorkingVehicle { get { return (Vehicle)WorkingObject; } private set { WorkingObject = value; } }


		#endregion

		#region methods and functions
		public override void renewtoStart()
		{

#if DEBUG
            Console.WriteLine("renewtoStart for CombatVehcile");
            Console.WriteLine(this.strID);
            Console.WriteLine(StartVehicle.Name);
#endif
            Vehicle ship = StartVehicle.Copy();
			ship.IsMemory = true;
			if (ship.Owner != StartVehicle.Owner)
				ship.Owner.Dispose(); // don't need extra empires!
			ship.Owner = StartVehicle.Owner;
#if DEBUG
            Console.WriteLine(ship.Name);
#endif
			// copy over the components individually so they can take damage without affecting the starting state
			ship.Components.Clear();
#if DEBUG
            Console.WriteLine("copying components");
#endif
			foreach (var comp in (StartVehicle.Components))
			{
				var ccopy = comp.Copy();
				ship.Components.Add(ccopy);
				ccopy.Container = ship;
#if DEBUG
                Console.WriteLine(ccopy.Name);
                Console.WriteLine("Container is " + ccopy.Container);
#endif
			}
#if DEBUG
            Console.WriteLine("Done");
#endif

			WorkingVehicle = ship;
			RefreshWeapons();

			foreach (var w in Weapons)
				w.nextReload = 1;

            base.renewtoStart();
#if DEBUG
            Console.WriteLine("Done");
#endif
		}

		protected override void RefreshWeapons()
		{
			var weapons = new List<CombatWeapon>();
#if DEBUG
            Console.WriteLine("RefreshingWeapons");
#endif
			foreach (Component weapon in WorkingVehicle.Weapons)
			{
				CombatWeapon wpn = new CombatWeapon(weapon);
				weapons.Add(wpn);
#if DEBUG
                Console.Write("Weapn Conatiner " + wpn.weapon.Container);
#endif
			}
			Weapons = weapons;
#if DEBUG
            Console.WriteLine("Done");
#endif
		}
		#endregion
	}
}
