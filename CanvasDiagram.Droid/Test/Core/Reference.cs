
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
	#region Reference

	public class Reference : Element
	{
		#region Constructor

		public Reference (int id, 
		                  string type, 
		                  Pin position,
		                  Element copy)
			: base(id, type)
		{
			Position = position;
			Copy = copy;
		}

		#endregion

		#region Properties

		public Pin Position { get; set; }
		public Element Copy { get; set; }

		#endregion

		public override void Render (object canvas) 
		{ 
			Copy.Render (canvas);
		}
	}

	#endregion
}
