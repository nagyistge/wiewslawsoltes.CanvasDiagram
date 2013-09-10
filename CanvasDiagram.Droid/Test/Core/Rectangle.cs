
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
	#region Rectangle

	public class Rectangle : Element
	{
		#region Constructor

		public Rectangle (int id, 
						  string type, 
		                  RectangleStyle style,
						  Pin topLeft,
						  Pin bottomRight)
			: base(id, type)
		{
			Style = style;
			TopLeft = topLeft;
			BottomRight = bottomRight;
		}

		#endregion

		#region Properties

		public RectangleStyle Style { get; set; }
		public Pin TopLeft { get; set; }
		public Pin BottomRight { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}
	
	#endregion
}
