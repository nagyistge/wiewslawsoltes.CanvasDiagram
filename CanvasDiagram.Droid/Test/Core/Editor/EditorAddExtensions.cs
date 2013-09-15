
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
	#region Editor Add Extensions

	public static class EditorAddExtensions
	{
		#region Add Style

		public static void Add (this Context ctx, PinStyle style)
		{
			if (!ctx.Model.PinStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to PinStyles dictionary.");
		}

		public static void Add (this Context ctx, LineStyle style)
		{
			if (!ctx.Model.LineStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to LineStyles dictionary.");
		}

		public static void Add (this Context ctx, RectangleStyle style)
		{
			if (!ctx.Model.RectangleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to RectangleStyles dictionary.");
		}

		public static void Add (this Context ctx, CircleStyle style)
		{
			if (!ctx.Model.CircleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to CircleStyles dictionary.");
		}

		public static void Add (this Context ctx, ArcStyle style)
		{
			if (!ctx.Model.ArcStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to ArcStyles dictionary.");
		}

		public static void Add (this Context ctx, TextStyle style)
		{
			if (!ctx.Model.TextStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to TextStyles dictionary.");
		}

		#endregion

		#region Add Primitive

		public static void Add (this Context ctx, Pin pin)
		{
			if (!ctx.Model.Pins.TryAdd (pin.Id, pin))
				throw new ArgumentException ("Failed Add to Pins dictionary.");
		}

		public static void Add (this Context ctx, Line line)
		{
			if (!ctx.Model.Lines.TryAdd (line.Id, line))
				throw new ArgumentException ("Failed Add to Lines dictionary.");
		}

		public static void Add (this Context ctx, Rectangle rectangle)
		{
			if (!ctx.Model.Rectangles.TryAdd (rectangle.Id, rectangle))
				throw new ArgumentException ("Failed Add to Rectangles dictionary.");
		}

		public static void Add (this Context ctx, Circle circle)
		{
			if (!ctx.Model.Circles.TryAdd (circle.Id, circle))
				throw new ArgumentException ("Failed Add to Circles dictionary.");
		}

		public static void Add (this Context ctx, Arc arc)
		{
			if (!ctx.Model.Arcs.TryAdd (arc.Id, arc))
				throw new ArgumentException ("Failed Add to Arcs dictionary.");
		}

		public static void Add (this Context ctx, Text text)
		{
			if (!ctx.Model.Texts.TryAdd (text.Id, text))
				throw new ArgumentException ("Failed Add to Texts dictionary.");
		}

		#endregion

		#region Add Custom

		public static void Add (this Context ctx, Custom custom)
		{
			if (!ctx.Model.CustomElements.TryAdd (custom.Id, custom))
				throw new ArgumentException ("Failed Add to CustomElements dictionary.");
		}

		#endregion
	}

	#endregion
}
