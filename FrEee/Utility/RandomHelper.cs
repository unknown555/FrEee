using System;

namespace FrEee.Utility
{
	/// <summary>
	/// Generates random numbers.
	/// </summary>
	public static class RandomHelper
	{
		private static Random rng = new Random();

		/// <summary>
		/// Generates a random number >= 0 but less than the upper bound.
		/// </summary>
		/// <param name="upper">The upper bound.</param>
		/// <returns></returns>
		public static int Next(int upper)
		{
			return rng.Next(upper);
		}

		/// <summary>
		/// Generates a random number >= 0 but less than the upper bound.
		/// </summary>
		/// <param name="upper">The upper bound.</param>
		/// <returns></returns>
		public static long Next(long upper)
		{
			return Next(upper / int.MaxValue) * int.MaxValue + Next(upper % int.MaxValue);
		}

		/// <summary>
		/// Generates a random number within a range (inclusive).
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum.</param>
		/// <returns></returns>
		public static int Range(int min, int max)
		{
			return rng.Next(min, max + 1);
		}

		/// <summary>
		/// Generates a random number >= 0 but less than the upper bound.
		/// </summary>
		/// <param name="upper">The upper bound.</param>
		/// <returns></returns>
		public static double Next(double upper)
		{
			return rng.NextDouble() * upper;
		}
	}
}
