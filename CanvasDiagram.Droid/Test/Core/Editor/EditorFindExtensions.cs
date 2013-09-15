
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
	#region Editor Find Extensions

	public static class EditorFindExtensions
	{
		#region Find Style

		public static PinStyle FindPinStyle (this Context ctx, int styleId)
		{
			PinStyle style;
			if (!ctx.Model.PinStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find PinStyle.");
			return style;
		}

		public static LineStyle FindLineStyle (this Context ctx, int styleId)
		{
			LineStyle style;
			if (!ctx.Model.LineStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find LineStyle.");
			return style;
		}

		public static RectangleStyle FindRectangleStyle (this Context ctx, int styleId)
		{
			RectangleStyle style;
			if (!ctx.Model.RectangleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find RectangleStyle.");
			return style;
		}

		public static CircleStyle FindCircleStyle (this Context ctx, int styleId)
		{
			CircleStyle style;
			if (!ctx.Model.CircleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find CircleStyle.");
			return style;
		}

		public static ArcStyle FindArcStyle (this Context ctx, int styleId)
		{
			ArcStyle style;
			if (!ctx.Model.ArcStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find ArcStyles.");
			return style;
		}

		public static TextStyle FindTextStyle (this Context ctx, int styleId)
		{
			TextStyle style;
			if (!ctx.Model.TextStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find TextStyles.");
			return style;
		}

		#endregion

		#region Find Primitive

		public static Pin FindPin (this Context ctx, int id)
		{
			Pin pin;
			if (!ctx.Model.Pins.TryGetValue (id, out pin))
				throw new ArgumentException ("Failed to find Pin");
			return pin;
		}

		public static Line FindLine (this Context ctx, int id)
		{
			Line line;
			if (!ctx.Model.Lines.TryGetValue (id, out line))
				throw new ArgumentException ("Failed to find Line");
			return line;
		}

		public static Rectangle FindRectangle (this Context ctx, int id)
		{
			Rectangle rectangle;
			if (!ctx.Model.Rectangles.TryGetValue (id, out rectangle))
				throw new ArgumentException ("Failed to find Rectangle");
			return rectangle;
		}

		public static Circle FindCircle (this Context ctx, int id)
		{
			Circle circle;
			if (!ctx.Model.Circles.TryGetValue (id, out circle))
				throw new ArgumentException ("Failed to find Circle");
			return circle;
		}

		public static Arc FindArc (this Context ctx, int id)
		{
			Arc arc;
			if (!ctx.Model.Arcs.TryGetValue (id, out arc))
				throw new ArgumentException ("Failed to find Arc");
			return arc;
		}

		public static Text FindText (this Context ctx, int id)
		{
			Text text;
			if (!ctx.Model.Texts.TryGetValue (id, out text))
				throw new ArgumentException ("Failed to find Text");
			return text;
		}

		#endregion
	}

	#endregion
}
