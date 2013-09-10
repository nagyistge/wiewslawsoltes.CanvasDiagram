
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
	#region Pin

	public class Pin : Element
	{
		#region Constructor

		public Pin (int id, 
		            string type, 
		            PinStyle style,
		            float x,
		            float y)
			: base(id, type)
		{
			Style = style;
			X = x;
			Y = y;
		}

		#endregion

		#region Properties

		public PinStyle Style { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public List<Line> Lines { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}
	
	#endregion
}
