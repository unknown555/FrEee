﻿using System;
using FrEee.Utility.Extensions;

using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Vehicles;
using FrEee.Game.Objects.Combat;
using FrEee.Game.Objects.Technology;
using FrEee.Modding;

using NewtMath.f16;
using FixMath.NET;
using FrEee.Game.Enumerations;
using FrEee.Game.Objects.Civilization;
using FrEee.Utility;


namespace FrEee.Game.Objects.Combat2
{
    public class CombatSeeker : CombatObject, ITargetable
    {
        public CombatSeeker(CombatObject attacker, CombatWeapon launcher, int ID)
            : base(null, new PointXd(attacker.cmbt_loc), new PointXd(attacker.cmbt_vel), ID, "SKR")
        {
			WorkingObject = this;
            SeekingWeaponInfo skrinfo = (SeekingWeaponInfo)launcher.weapon.Template.ComponentTemplate.WeaponInfo;
			Hitpoints = MaxHitpoints = skrinfo.SeekerDurability;
            cmbt_mass = (Fix16)Hitpoints;//(Fix16)s.MaxHitpoints; // sure why not?
            int wpnskrspd = skrinfo.SeekerSpeed;
            int wpnskrEvade = Mod.Current.Settings.SeekerEvasion;
            maxfowardThrust = (Fix16)wpnskrspd * this.cmbt_mass * (Fix16)0.15;
            maxStrafeThrust = (Fix16)wpnskrspd * this.cmbt_mass * (Fix16)0.1 / ((Fix16)4 - (Fix16)wpnskrEvade * (Fix16)0.01);
            maxRotate.Degrees = ((Fix16)wpnskrspd * this.cmbt_mass * (Fix16)0.1) / ((Fix16)2.5 - (Fix16)wpnskrEvade * (Fix16)0.01);
            

            cmbt_thrust = new PointXd(0, 0, 0);
            cmbt_accel = new PointXd(0, 0, 0);

            newDice(ID);
#if DEBUG
            Console.WriteLine("MaxAccel = " + maxfowardThrust / cmbt_mass);
#endif
            this.launcher = launcher;
        }

        #region fields & properties
        public CombatTakeFireEvent seekertargethit { get; set; }

        //the component that fired the missile.
        public CombatWeapon launcher { get; private set; }

		public WeaponTargets WeaponTargetType
		{
			get { return WeaponTargets.Seeker; }
		}

		public int Hitpoints
		{
			get;
			set;
		}

		public int NormalShields
		{
			get;
			set;
		}

		public int PhasedShields
		{
			get;
			set;
		}

		public int MaxHitpoints
		{
			get;
			private set;
		}

		public int MaxNormalShields
		{
			get { return 0; }
		}

		public int MaxPhasedShields
		{
			get { return 0; }
		}

		public int ShieldHitpoints
		{
			get { return NormalShields + PhasedShields; }
		}

		public int ArmorHitpoints
		{
			get { return 0; }
		}

		public int HullHitpoints
		{
			get { return Hitpoints; }
		}

		public int MaxShieldHitpoints
		{
			get { return MaxNormalShields + MaxPhasedShields; }
		}

		public int MaxArmorHitpoints
		{
			get { return 0; }
		}

		public int MaxHullHitpoints
		{
			get { return MaxHitpoints; }
		}

		public bool IsDestroyed
		{
			get { return Hitpoints <= 0; }
		}

		public int HitChance
		{
			get { return 1; }
		}

		public Empire Owner
		{
			// seeker owner is irrelevant outside of combat, and we have CombatEmpire for that
			get { return null; }
		}

		public bool IsDisposed
		{
			get;
			set;
		}

		public int Evasion
		{
			get
			{
				// TODO - per-seeker evasion settings
				return Mod.Current.Settings.SeekerEvasion;
			}
		}

        #endregion

        #region methods & functions

        public override void renewtoStart()
        {
            //do nothing. this should not ever happen here.
        }

        public override void helm()
        {
            
            Compass angletoWaypoint = new Compass(this.cmbt_loc, this.waypointTarget.cmbt_loc); //relitive to me. 

            Tuple<Compass, bool?> nav = Nav(angletoWaypoint);
            Compass angletoturn = nav.Item1;
            bool? thrustToWaypoint = nav.Item2;

            turnship(angletoturn, angletoWaypoint);

            thrustship(angletoturn, true);            
        }

        protected override Tuple<Compass, bool?> Nav(Compass angletoWaypoint)
        {          
            Compass angletoturn = new Compass();
            bool? thrustTowards = true;
            Tuple<Compass, bool?> nav = new Tuple<Compass, bool?>(angletoturn, thrustTowards);

            combatWaypoint wpt = this.waypointTarget;
            angletoturn = new Compass(angletoWaypoint.Radians - this.cmbt_head.Radians);
            PointXd vectortowaypoint = this.cmbt_loc - this.waypointTarget.cmbt_loc;

            Fix16 acceleration = maxfowardThrust * cmbt_mass;
            Fix16 startV = Trig.distance(cmbt_vel, wpt.cmbt_vel);
            Fix16 distance = vectortowaypoint.Length;
            Fix16[] quad = NMath.quadratic(acceleration, startV, distance);
            Fix16 ttt;
            if (quad[2] == 1)
            {
                ttt = Fix16.Min(quad[0], quad[1]);
            }
            else
                ttt = quad[0];
            Fix16 endV = startV + acceleration * ttt;
#if DEBUG
            Console.WriteLine("seeker ttt: " + ttt);
#endif

            return nav;
        }

		public void ReplenishShields(int? amount = null)
		{
			// seekers don't have shields
		}

		public int? Repair(int? amount = null)
		{
			if (amount == null)
			{
				Hitpoints = MaxHitpoints;
				return null;
			}
			if (amount + Hitpoints > MaxHitpoints)
			{
				amount -= (MaxHitpoints - Hitpoints);
				Hitpoints = MaxHitpoints;
				return amount;
			}
			Hitpoints += amount.Value;
			return 0;
		}

		public int TakeDamage(DamageType dmgType, int damage, PRNG dice = null)
		{
			// TODO - damage types
			if (damage > Hitpoints)
			{
				damage -= Hitpoints;
				Hitpoints = 0;
				return damage;
			}
			Hitpoints -= damage;
			return 0;
		}

		public void Dispose()
		{
			Hitpoints = 0;
			IsDisposed = true;
		}

		public override int handleShieldDamage(int damage)
		{
			// seekers don't have shields, just leak the damage
			return damage;
		}

		public override int handleComponentDamage(int damage, DamageType damageType, PRNG attackersdice)
		{
			return TakeDamage(damageType, damage, null);
		}

        /*/// <summary>
        /// was missilefirecontrol in battlespace.
        /// </summary>
        /// <param name="battletick"></param>
        /// <param name="comSek"></param>

        public override void firecontrol(int battletick)
        {
            Fix16 locdistance = Trig.distance(comSek.cmbt_loc, comSek.weaponTarget[0].cmbt_loc);
            if (locdistance <= comSek.cmbt_vel.Length)//erm, I think? (if we're as close as we're going to get in one tick) could screw up at high velocities.
            {
                if (!IsReplay)
                {
                    CombatTakeFireEvent evnt = comSek.seekertargethit;
                    evnt.IsHit = true;
                    evnt.Tick = battletick;
                }
                Component launcher = comSek.launcher.weapon;
                CombatObject target = comSek.weaponTarget[0];
                if (target is ControlledCombatObject)
                {
                    ControlledCombatObject ccTarget = (ControlledCombatObject)target;
                    var target_icomobj = ccTarget.WorkingObject;
                    var shot = new Combat.Shot(launcher, target_icomobj, 0);
                    //defender.TakeDamage(weapon.Template.ComponentTemplate.WeaponInfo.DamageType, shot.Damage, battle);
                    int damage = shot.Damage;
                    combatDamage(battletick, target, comSek.launcher, damage, comSek.getDice());
                    if (target_icomobj.MaxNormalShields < target_icomobj.NormalShields)
                        target_icomobj.NormalShields = target_icomobj.MaxNormalShields;
                    if (target_icomobj.MaxPhasedShields < target_icomobj.PhasedShields)
                        target_icomobj.PhasedShields = target_icomobj.MaxPhasedShields;
                }

                DeadNodes.Add(comSek);
                CombatNodes.Remove(comSek);
            }
            else if (battletick > comSek.deathTick)
            {
                DeadNodes.Add(comSek);
                CombatNodes.Remove(comSek);
            }
        }
         */

        #endregion
	}
}
