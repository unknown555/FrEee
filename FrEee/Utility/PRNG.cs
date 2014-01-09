﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrEee.Utility.Extensions;

namespace FrEee.Utility
{
    /// <summary>
    /// PredictibleRandomNumberGenerator.
    /// pretty much the same as RandomHelper, but not static, and requires a seed.
    /// </summary>
    public class PRNG
    {
        Random prng;
        public PRNG(int seed)
        {
            this.Seed = seed;
            prng = new Random(seed);
        }

		/// <summary>
		/// The number used to seed this PRNG.
		/// </summary>
		public int Seed { get; private set; }

		/// <summary>
		/// The number of times that this PRNG has been called.
		/// </summary>
		public long Iteration { get; private set; }

        public int Next(int upper)
        {
			Iteration++;
            return prng.Next(upper);
        }

        /// <summary>
        /// Generates a random number >= 0 but less than the upper bound.
		/// Increments Iteration 3 times due to needing to generate 3 integers.
		/// TODO - find a way to do this with only 2 integers
        /// </summary>
        /// <param name="upper">The upper bound.</param>
        /// <returns></returns>
        public long Next(long upper)
        {
			// don't Iteration++ here; we're already calling Next
            return Next((int)(upper / (long)int.MaxValue / 4L)) * (long)int.MaxValue * 4L + Next((int)(upper % ((long)int.MaxValue))) * 2 + Next(2);
        }

        /// <summary>
        /// Generates a random number within a range (inclusive).
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public int Range(int min, int max)
        {
			Iteration++;
            return prng.Next(min, max + 1);
        }

        /// <summary>
        /// Generates a random number within a range (inclusive).
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public long Range(long min, long max)
        {
			// don't Iteration++ here; we're already calling Next
            return Next(max - min + 1) + min;
        }

        /// <summary>
        /// Generates a random number >= 0 but less than the upper bound.
        /// </summary>
        /// <param name="upper">The upper bound.</param>
        /// <returns></returns>
        public double Next(double upper)
        {
			Iteration++;
            return prng.NextDouble() * upper;
        }

		public static bool operator ==(PRNG r1, PRNG r2)
		{
			if (r1.IsNull() && r2.IsNull())
				return true;
			if (r1.IsNull() || r2.IsNull())
				return false;
			return r1.Seed == r2.Seed && r1.Iteration == r2.Iteration;
		}

		public static bool operator !=(PRNG r1, PRNG r2)
		{
			return !(r1 == r2);
		}

		public override bool Equals(object obj)
		{
 			if (obj is PRNG)
				return this == (PRNG)obj;
			return false;
		}

		public override int GetHashCode()
		{
			return Seed.GetHashCode() ^ Iteration.GetHashCode();
		}

		public override string ToString()
		{
			return "Seed: " + Seed + ", Iteration: " + Iteration;
		}
    }
}