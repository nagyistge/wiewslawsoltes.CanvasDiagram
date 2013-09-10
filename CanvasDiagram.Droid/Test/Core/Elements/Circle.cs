
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
	#region Circle

	public class Circle : Element
	{
		#region Constructor

		public Circle (int id, 
		               string type, 
		               CircleStyle style,
		               Pin center,
		               Pin radius)
			: base(id, type)
		{
			Style = style;
			Center = center;
			Radius = radius;
		}

		#endregion

		#region Properties

		public CircleStyle Style { get; set; }
		public Pin Center { get; set; }
		public Pin Radius { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}
	
	#endregion
}
