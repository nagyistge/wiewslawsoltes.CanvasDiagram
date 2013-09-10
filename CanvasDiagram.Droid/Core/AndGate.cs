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
	#region AndGate

	public class AndGate : Element
	{ 
		#region Constructor

		public AndGate (int id, float x, float y)
			: base()
		{
			Initialize (id, x, y);
		}

		#endregion

		private void Initialize (int id, float x, float y)
		{
			float width = 30f;
			float height = 30f;
			float radius = 4f;
			float hitOffset = 3f;

			Id = id;
			Width = width;
			Height = height;
			X = x;
			Y = y;
			Bounds = new RectF (x, y, x + width, y + height);
			ShowPins = false;
			Pins = new List<Pin> ();

			Pins.Add (new Pin (0, this, x + 0f, y + (height / 2f), radius, hitOffset)); // left
			Pins.Add (new Pin (1, this, x + width, y + (height / 2f), radius, hitOffset)); // right
			Pins.Add (new Pin (2, this, x + (width / 2f), y + 0f, radius, hitOffset)); // top
			Pins.Add (new Pin (3, this, x + (width / 2f), y + height, radius, hitOffset)); // bottom
		}
	}

	#endregion
}
