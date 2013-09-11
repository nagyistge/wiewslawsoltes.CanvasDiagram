
#region References

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Android.Graphics;
using CanvasDiagram.Core.Test;

#endregion

namespace CanvasDiagram.Droid.Renderers
{
	#region Android Canvas Renderers

	public class LineRenderer : IRenderer<Line, LineStyle>
	{
		#region IRenderer implementation

		private Paint paint = new Paint();
		private LineStyle style = null;

		public void Initialize (LineStyle style)
		{
			this.style = style;
			this.paint.Color = Color.Argb (style.ColorA, style.ColorR, style.ColorG, style.ColorB);
			this.paint.AntiAlias = true;
			this.paint.StrokeWidth = style.StrokeWidth;
			this.paint.StrokeCap = Paint.Cap.Square;
			this.paint.SetStyle (style.IsFilled ? Paint.Style.FillAndStroke : Paint.Style.Stroke);
		}

		public void Render (object canvas, Line element)
		{
			var _canvas = canvas as Canvas;
			_canvas.DrawLine (element.Start.X, element.Start.Y, element.End.X, element.End.Y, paint);
		}

		#endregion
	}

	#endregion
}
