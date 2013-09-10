
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
	#region Arc

	public class Arc : Element
	{
		#region Constructor

		public Arc (int id, 
		            string type, 
		            ArcStyle style,
		            Pin start,
		            Pin end,
		            Pin center)
			: base(id, type)
		{
			Style = style;
			Start = start;
			End = end;
			Center = center;
		}

		#endregion

		#region Properties

		public ArcStyle Style { get; set; }
		public Pin Start { get; set; }
		public Pin End { get; set; }
		public Pin Center { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}
	
	#endregion
}
