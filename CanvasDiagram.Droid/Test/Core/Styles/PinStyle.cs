
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
	#region PinStyle

	public class PinStyle : BaseStyle<Pin, PinStyle>
	{
		#region Constructor

		public PinStyle (IRenderer<Pin, PinStyle> renderer,
		                 int id, 
		                 string type, 
		                 int colorA,
		                 int colorR,
		                 int colorG,
		                 int colorB,
		                 bool isFilled,
		                 float strokeWidth,
		                 float radius) 
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
			Renderer = renderer;
			Radius = radius;
		}

		#endregion

		#region Properties

		public float Radius { get; set; }

		#endregion
	}

	#endregion
}
