
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
	#region Element

	public abstract class Element
	{
		#region Constructor

		public Element (int id, 
		                string type)
		{
			Id = id;
			Type = type;
		}

		#endregion

		#region Properties

		public int Id { get; set; }
		public string Type { get; set; }

		#endregion

		public abstract void Render(object canvas);
	}

	#endregion
}
