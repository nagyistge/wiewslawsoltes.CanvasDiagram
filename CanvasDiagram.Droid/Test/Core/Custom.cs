
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
	#region Custom

	public class Custom : Element
	{
		#region Properties

		public List<Element> Primitives { get; set; }
		public List<Custom> Customs { get; set; }

		#endregion

		#region Constructor

		public Custom (int id, 
		               string type)
			: base(id, type)
		{
			Primitives = new List<Element> ();
			Customs = new List<Custom> ();
		}

		#endregion

		public override void Render (object canvas)
		{ 
			int count, i;
			var primitives = Primitives;
			var customs = Customs;

			count = Primitives.Count;
			for (i = 0; i < count; i++)
				primitives [i].Render (canvas);

			count = Customs.Count;
			for (i = 0; i < count; i++)
				customs [i].Render (canvas);
		}
	}

	#endregion
}
