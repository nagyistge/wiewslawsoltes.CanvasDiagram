#region References

using System;
using Android.Graphics;

#endregion

namespace CanvasDiagram.Droid.Core
{
	#region MathUtil

	public static class MathUtil
	{
		public static float LineDistance (float x1, float y1, float x2, float y2)
		{
			float dx = x1 - x2;
			float dy = y1 - y2;
			return (float)Math.Sqrt (dx * dx + dy * dy);
		}

		public static void LineMiddle (ref PointF point, float x1, float y1, float x2, float y2)
		{
			float x = x1 + x2;
			float y = y1 + y2;
			point.Set (x / 2f, y / 2f);
		}

		public static PointF ClosestPointOnLine(PointF a, PointF b, PointF p)
		{
			// en.wikipedia.org/wiki/Vector_projection
			return VectorAdd (VectorProject (VectorSubstract (p, a), VectorSubstract (b, a)), a);
		}

		public static float VectorDot(PointF a, PointF b)
		{
			return (a.X * b.X) + (a.Y * b.Y);
		}

		public static PointF VectorMultiply(PointF a, float scalar)
		{
			return new PointF(a.X * scalar, a.Y * scalar);
		}

		public static PointF VectorProject(PointF a, PointF b)
		{
			return VectorMultiply (b, VectorDot (a, b) / VectorDot (b, b));
		}

		public static PointF VectorSubstract(PointF a, PointF b)
		{
			return new PointF(a.X - b.X, a.Y - b.Y);
		}

		public static PointF VectorAdd(PointF a, PointF b)
		{
			return new PointF(a.X + b.X, a.Y + b.Y);
		}
	}

	#endregion
}
