
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
	#region ArcStyle

	public class ArcStyle : BaseStyle<Arc, ArcStyle>
	{
		#region Constructor

		public ArcStyle (IRenderer<Arc, ArcStyle> renderer,
		                 int id, 
		                 string type, 
		                 int colorA,
		                 int colorR,
		                 int colorG,
		                 int colorB,
		                 bool isFilled,
		                 float strokeWidth) 
			: base(renderer,
			       id, 
			       type, 
			       colorA,
			       colorR,
			       colorG,
			       colorB,
			       isFilled,
			       strokeWidth)
		{
		}

		#endregion
	}
	
	#endregion
}
