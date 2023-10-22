﻿using FrEee.Extensions;
using FrEee.Interfaces;
using FrEee.Objects.Space;
using FrEee.Utility;
using System;
using System.Drawing;
using System.Linq;

namespace FrEee.Setup.StarSystemPlacementStrategies
{
	/// <summary>
	/// Places stars in a grid.
	/// </summary>
	[Serializable]
	public class GridStarSystemPlacementStrategy : IStarSystemPlacementStrategy
	{
		public Point? PlaceStarSystem(Galaxy galaxy, int buffer, Rectangle bounds, int starsLeft, PRNG dice)
		{
			var openPositions = bounds.GetAllPoints();
			foreach (var sspos in galaxy.StarSystemLocations.Select(sspos => sspos.Location))
				openPositions = openPositions.BlockOut(sspos, buffer);
			if (!openPositions.Any())
				return null;

			int totalStars = starsLeft + galaxy.StarSystemLocations.Count;
			var xfactor = Math.Sqrt(totalStars) * bounds.Height / bounds.Width;
			var yfactor = Math.Sqrt(totalStars) * bounds.Width / bounds.Height;
			var xstars = (int)(totalStars / xfactor);
			var ystars = (int)(totalStars / yfactor);

			if (xstars * ystars <= galaxy.StarSystemLocations.Count)
				return null;

			int row = galaxy.StarSystemLocations.Count % xstars;
			int col = galaxy.StarSystemLocations.Count / xstars;
			int rowsize, colsize;
			if (xstars == 1)
				rowsize = bounds.Width / 2;
			else
				rowsize = bounds.Width / (xstars - 1);
			if (ystars == 1)
				colsize = bounds.Height / 2;
			else
				colsize = bounds.Height / (ystars - 1);

			var idealPos = new Point(row * rowsize + bounds.Left, col * colsize + bounds.Top);

			return openPositions.OrderBy(p => p.ManhattanDistance(idealPos)).First();
		}
	}
}