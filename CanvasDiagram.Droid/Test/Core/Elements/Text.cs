
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
	#region Text

	public class Text : Element
	{
		#region Constructor

		public Text (int id, 
		             string type, 
		             TextStyle style,
		             Pin position)
			: base(id, type)
		{
			Style = style;
			Position = position;
		}

		#endregion

		#region Properties

		public TextStyle Style { get; set; }
		public Pin Position { get; set; }

		#endregion

		public override void Render (object canvas)
		{
			Style.Render (canvas, this);
		}
	}

	#endregion
}
