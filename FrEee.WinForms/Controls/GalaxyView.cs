using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Space;
using FrEee.Utility.Extensions;

namespace FrEee.WinForms.Controls
{
	/// <summary>
	/// Displays a galaxy map.
	/// </summary>
	public partial class GalaxyView : Control
	{
		public GalaxyView()
		{
			InitializeComponent();
			BackColor = Color.Black;
			this.SizeChanged += GalaxyView_SizeChanged;
			this.MouseClick += GalaxyView_MouseClick;
		}

		/// <summary>
		/// Delegate for events related to star system selection.
		/// </summary>
		/// <param name="sender">The galaxy view triggering the event.</param>
		/// <param name="sector">The star system selected/deselected/etc.</param>
		public delegate void StarSystemSelectionDelegate(GalaxyView sender, StarSystem starSystem);

		/// <summary>
		/// Occurs when the user clicks with the left mouse button on a star system or on empty space.
		/// </summary>
		public event StarSystemSelectionDelegate StarSystemClicked;

		/// <summary>
		/// Occurs when the selected star system changes.
		/// </summary>
		public event StarSystemSelectionDelegate StarSystemSelected;

		void GalaxyView_MouseClick(object sender, MouseEventArgs e)
		{
			if (StarSystemClicked != null)
				StarSystemClicked(this, GetStarSystemAtPoint(e.Location));
		}

		/// <summary>
		/// Gets the star system at specific screen coordinates.
		/// </summary>
		/// <param name="p">The screen coordinates.</param>
		/// <returns></returns>
		public StarSystem GetStarSystemAtPoint(Point p)
		{
			if (Galaxy.Current == null)
				return null; // no such sector
			var drawsize = StarSystemDrawSize;
			// TODO - don't cut off the systems on the edges
			var x = (int)Math.Round((p.X - Width / 2f) / drawsize);
			var y = (int)Math.Round((p.Y - Height / 2f) / drawsize);
			var p2 = new Point(x, y);
			var ssloc = Galaxy.Current.StarSystemLocations.FirstOrDefault(ssl => ssl.Location == p2);
			if (ssloc == null)
				return null;
			return ssloc.Item;
		}

		/// <summary>
		/// The size at which each star system will be drawn, in pixels.
		/// </summary>
		public float StarSystemDrawSize
		{
			get
			{
				if (Galaxy.Current == null)
					return 0;
				return (float)Math.Min(Width, Height) / ((float)Math.Max(Galaxy.Current.Width, Galaxy.Current.Height));
			}
		}

		void GalaxyView_SizeChanged(object sender, EventArgs e)
		{
			Invalidate();
		}

		private StarSystem selectedStarSystem;

		public StarSystem SelectedStarSystem
		{
			get { return selectedStarSystem; }
			set
			{
				selectedStarSystem = value;
				if (StarSystemSelected != null)
					StarSystemSelected(this, value);
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);

			pe.Graphics.Clear(BackColor);

			pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			if (Galaxy.Current != null)
			{
				var drawsize = StarSystemDrawSize;
				var whitePen = new Pen(Color.White);

				// draw star systems
				foreach (var ssl in Galaxy.Current.StarSystemLocations)
				{
					// where will we draw the star system?
					// TODO - don't cut off the systems on the edges
					var x = ssl.Location.X;
					var y = ssl.Location.Y;
					var drawx = x * drawsize + Width / 2f;
					var drawy = y * drawsize + Height / 2f;

					// find star system
					var sys = ssl.Item;

					// draw circle for star system
					// do SE3-style split circles for contested systems because they are AWESOME!
					var owners = sys.FindSpaceObjects<ISpaceObject>().SelectMany(g => g).Select(g => g.Owner).Distinct().Where(o => o != null);
					if (owners.Count() == 0)
						pe.Graphics.DrawEllipse(new Pen(Color.Gray), drawx - drawsize / 2f, drawy - drawsize / 2f, drawsize, drawsize);
					else
					{
						var arcSize = 360f / owners.Count();
						int i = 0;
						foreach (var owner in owners)
						{
							pe.Graphics.DrawArc(new Pen(owner.Color), drawx - drawsize / 2f, drawy - drawsize / 2f, drawsize, drawsize, i * arcSize, arcSize);
							i++;
						}
					}

					// TODO - draw star system name?

					// draw selection reticule (just a square for now)
					if (sys == SelectedStarSystem)
						pe.Graphics.DrawRectangle(whitePen, drawx - drawsize / 2f - 1, drawy - drawsize / 2f - 1, drawsize + 2, drawsize + 2);
				}

				// draw warp points
				foreach (var ssl in Galaxy.Current.StarSystemLocations)
				{
					var startPos = new PointF
					(
						ssl.Location.X * drawsize + Width / 2f,
						ssl.Location.Y * drawsize + Height / 2f
					);
					foreach (var wp in ssl.Item.FindSpaceObjects<WarpPoint>().Flatten())
					{
						var endPos = new PointF
						(
							wp.TargetStarSystemLocation.Location.X * drawsize + Width / 2f,
							wp.TargetStarSystemLocation.Location.Y * drawsize + Height / 2f
						);

						// overlapping systems or same system
						if (startPos == endPos)
							continue;

						// push the ends out past the system circles
						var dx = endPos.X - startPos.X;
						var dy = endPos.Y - startPos.Y;
						var length = Math.Max(Math.Abs(dx), Math.Abs(dy));
						var ndx = dx / length * drawsize / 2f;
						var ndy = dy / length * drawsize / 2f;
						var realStartPos = new PointF(startPos.X + ndx, startPos.Y + ndy);
						var realEndPos = new PointF(endPos.X - ndx, endPos.Y - ndy);

						// draw line
						pe.Graphics.DrawLine(whitePen, realStartPos, realEndPos); 

						// draw arrow
						var angle = startPos.AngleTo(endPos);
						var radians = Math.PI * angle / 180d;
						var adx1 = -(float)Math.Sin(radians + Math.PI / 6d) * drawsize / 2f;
						var ady1 = (float)Math.Cos(radians + Math.PI / 6d) * drawsize / 2f;
						var arrowEndPos1 = new PointF(realEndPos.X + adx1, realEndPos.Y + ady1);
						var adx2 = -(float)Math.Sin(radians - Math.PI / 6d) * drawsize / 2f;
						var ady2 = (float)Math.Cos(radians - Math.PI / 6d) * drawsize / 2f;
						var arrowEndPos2 = new PointF(realEndPos.X + adx2, realEndPos.Y + ady2);
						pe.Graphics.DrawLine(whitePen, realEndPos, arrowEndPos1);
						pe.Graphics.DrawLine(whitePen, realEndPos, arrowEndPos2);
					}
				}
			}
		}
	}
}
