#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

#endregion

namespace CanvasDiagram.Droid.Core
{
	#region Pin

	public class Pin : Element
	{
		#region Properties

		public float Radius { get; set; }
		public float HitOffset { get; set; }
		public List<Wire> Wires { get; set; }

		#endregion

		#region Constructor
		
		public Pin (int id, Element parent, float x, float y, float radius, float offset)
			: base()
		{
			Initialize (id, parent, x, y, radius, offset);
		}

		#endregion

		private void Initialize (int id, Element parent, float x, float y, float radius, float offset)
		{
			Id = id;
			X = x;
			Y = y;
			Radius = radius;
			HitOffset = offset;
			Bounds = new RectF (x - radius - offset, 
			                    y - radius - offset, 
			                    x + radius + offset, 
			                    y + radius + offset);
			Parent = parent;
			Wires = new List<Wire> ();
		}

		public override void Update (float dx, float dy)
		{
			float x = X + dx;
			float y = Y + dy;
			X = x; 
			Y = y;
			Bounds.Set (x - Radius - HitOffset, 
			            y - Radius - HitOffset, 
			            x + Radius + HitOffset, 
			            y + Radius + HitOffset);
		} 
	}

	#endregion
}
