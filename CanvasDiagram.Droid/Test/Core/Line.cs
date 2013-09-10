
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
	#region Line

	public class Line : Element
	{
		#region Constructor

		public Line (int id, 
		             string type, 
		             LineStyle style,
		             Pin start,
		             Pin end)
			: base(id, type)
		{
			Style = style;
			Start = start;
			End = end;
		}

		#endregion

		#region Properties

		public LineStyle Style { get; set; }
		public Pin Start { get; set; }
		public Pin End { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}
	
	#endregion
}
