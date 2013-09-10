
#region References

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

#endregion

namespace CanvasDiagram.Core.Test
{
	#region Model

	public class Model
	{
		#region Properties

		public ConcurrentDictionary<int, PinStyle> PinStyles { get; set; }
		public ConcurrentDictionary<int, LineStyle> LineStyles { get; set; }
		public ConcurrentDictionary<int, RectangleStyle> RectangleStyles { get; set; }
		public ConcurrentDictionary<int, ArcStyle> ArcStyles { get; set; }
		public ConcurrentDictionary<int, CircleStyle> CircleStyles { get; set; }
		public ConcurrentDictionary<int, TextStyle> TextStyles { get; set; }

		public ConcurrentDictionary<int, Pin> Pins { get; set; }
		public ConcurrentDictionary<int, Line> Lines { get; set; }
		public ConcurrentDictionary<int, Rectangle> Rectangles { get; set; }
		public ConcurrentDictionary<int, Arc> Arcs { get; set; }
		public ConcurrentDictionary<int, Circle> Circles { get; set; }
		public ConcurrentDictionary<int, Text> Texts { get; set; }

		public ConcurrentDictionary<int, Pin> ElementPins { get; set; }
		public ConcurrentDictionary<int, Pin> ConnectorPins { get; set; }

		public ConcurrentDictionary<int, Custom> CustomElements { get; set; }
		public ConcurrentDictionary<int, Reference> ReferenceElements { get; set; }

		#endregion

		#region Constructor

		public Model ()
		{
			PinStyles = new ConcurrentDictionary<int, PinStyle> ();
			LineStyles = new ConcurrentDictionary<int, LineStyle> ();
			RectangleStyles = new ConcurrentDictionary<int, RectangleStyle> ();
			ArcStyles = new ConcurrentDictionary<int, ArcStyle> ();
			CircleStyles = new ConcurrentDictionary<int, CircleStyle> ();
			TextStyles = new ConcurrentDictionary<int, TextStyle> ();

			Pins = new ConcurrentDictionary<int, Pin> ();
			Lines = new ConcurrentDictionary<int, Line> ();
			Rectangles = new ConcurrentDictionary<int, Rectangle> ();
			Arcs = new ConcurrentDictionary<int, Arc> ();
			Circles = new ConcurrentDictionary<int, Circle> ();
			Texts= new ConcurrentDictionary<int, Text> ();

			ElementPins = new ConcurrentDictionary<int, Pin> ();
			ConnectorPins = new ConcurrentDictionary<int, Pin> ();

			CustomElements = new ConcurrentDictionary<int, Custom> ();
			ReferenceElements = new ConcurrentDictionary<int, Reference> ();
		}

		#endregion
	}

	#endregion
}
