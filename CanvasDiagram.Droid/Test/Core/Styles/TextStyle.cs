
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
	#region TextStyle

	public class TextStyle : BaseStyle<Text, TextStyle>
	{
		#region Constructor

		public TextStyle (IRenderer<Text, TextStyle> renderer,
		                  int id, 
		                  string type, 
		                  int colorA,
		                  int colorR,
		                  int colorG,
		                  int colorB,
		                  bool isFilled,
		                  float strokeWidth,
		                  int horizontalAlignment,
		                  int verticalAlignment,
		                  float size) 
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
			HorizontalAlignment = horizontalAlignment;
			VerticalAlignment = verticalAlignment;
			Size = size;
		}

		#endregion

		#region Properties

		public int HorizontalAlignment { get; set; }
		public int VerticalAlignment { get; set; }
		public float Size { get; set; }

		#endregion
	}

	#endregion
}

