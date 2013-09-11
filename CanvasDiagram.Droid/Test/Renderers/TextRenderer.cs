
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

	public class TextRenderer : IRenderer<Text, TextStyle>
	{
		#region IRenderer implementation

		private Paint paint = new Paint();
		private TextStyle style = null;

		public void Initialize (TextStyle style)
		{
			this.style = style;
			this.paint.Color = Color.Argb (style.ColorA, style.ColorR, style.ColorG, style.ColorB);
			this.paint.AntiAlias = true;
			this.paint.StrokeWidth = style.StrokeWidth;
			this.paint.StrokeCap = Paint.Cap.Square;
			this.paint.SetStyle (style.IsFilled ? Paint.Style.FillAndStroke : Paint.Style.Stroke);
		}

		public void Render (object canvas, Text element)
		{
			var _canvas = canvas as Canvas;
			//
		}

		#endregion
	}

	#endregion
}
