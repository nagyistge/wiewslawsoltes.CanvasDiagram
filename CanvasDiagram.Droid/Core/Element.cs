#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;

#endregion

namespace CanvasDiagram.Droid.Core
{
	#region Element

	public abstract class Element
	{
		#region Properties

		public int Id { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public RectF Bounds { get; set; }
		public bool ShowPins { get; set; }
		public List<Pin> Pins { get; set; }
		public bool IsSelected { get; set; }
		public Element Parent { get; set; }

		#endregion

		#region Update

		public virtual void Update (float dx, float dy)
		{
			float x = X + dx;
			float y = Y + dy;
			X = x; 
			Y = y;
			Bounds.Set (x, y, x + Width, y + Height);

			if (Pins != null) 
			{
				for(int i = 0; i < Pins.Count; i++)
					Pins[i].Update (dx, dy);
			}
		}

		#endregion
	}

	#endregion
}
