
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
	#region BaseStyle

	public abstract class BaseStyle<E, S>
	{
		#region Constructor

		public BaseStyle (IRenderer<E, S> renderer,
		                  int id, 
		                  string type, 
		                  int colorA,
		                  int colorR,
		                  int colorG,
		                  int colorB,
		                  bool isFilled,
		                  float strokeWidth)
		{ 
			Renderer = renderer;
			Id = id; 
			Type = type;
			ColorA = colorA;
			ColorR = colorR;
			ColorG = colorG;
			ColorB = colorB;
			IsFilled = isFilled;
			StrokeWidth = strokeWidth;
		}

		#endregion

		#region Properties

		protected IRenderer<E, S> Renderer { get; set; }

		public int Id { get; set; }
		public string Type { get; set; }
		public int ColorA { get; set; }
		public int ColorR { get; set; }
		public int ColorG { get; set; }
		public int ColorB { get; set; }
		public bool IsFilled { get; set; }
		public float StrokeWidth { get; set; }

		#endregion
		
		public virtual void Render (object canvas, E element)
		{
			Renderer.Render (canvas, element);
		}
	}

	#endregion
}
