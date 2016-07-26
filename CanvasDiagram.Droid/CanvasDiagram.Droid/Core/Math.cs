using System;
using Android.Graphics;

namespace CanvasDiagram.Droid
{
    public static class VectorUtil
    {
        public static float Dot(PointF a, PointF b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static PointF Multiply(PointF a, float scalar)
        {
            return new PointF(a.X * scalar, a.Y * scalar);
        }

        public static PointF Project(PointF a, PointF b)
        {
            return Multiply(b, Dot(a, b) / Dot(b, b));
        }

        public static PointF Substract(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        public static PointF Add(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        public static PointF NearestPointOnLine(PointF a, PointF b, PointF p)
        {
            // http://en.wikipedia.org/wiki/Vector_projection
            return Add(
                Project(
                    Substract(p, a), 
                    Substract(b, a)), 
                a);
        }
    }

    public static class LineUtil
    {
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static void Middle(ref PointF point, float x1, float y1, float x2, float y2)
        {
            float x = x1 + x2;
            float y = y1 + y2;
            point.Set(x / 2f, y / 2f);
        }
    }

    public class PolygonF
    {
        public int Sides { get { return polySides; } }

        private float[] polyY, polyX;
        private int polySides;

        public PolygonF(float[] px, float[] py, int ps)
        {
            polyX = px;
            polyY = py;
            polySides = ps;
        }

        public void SetX(int index, float x)
        {
            if (polyX != null && index >= 0 && index < polySides)
                polyX[index] = x;
        }

        public void SetY(int index, float y)
        {
            if (polyY != null && index >= 0 && index < polySides)
                polyY[index] = y;
        }

        public float GetX(int index)
        {
            if (polyX != null && index >= 0 && index < polySides)
                return polyX[index];
            return float.NaN;
        }

        public float GetY(int index)
        {
            if (polyY != null && index >= 0 && index < polySides)
                return polyY[index];
            return float.NaN;
        }

        public bool Contains(float x, float y)
        {
            if (polyX == null || polyY == null || polySides == 0)
                return false;

            bool c = false;
            int i, j = 0;

            for (i = 0, j = polySides - 1; i < polySides; j = i++)
            {
                if (((polyY[i] > y) != (polyY[j] > y))
                        && (x < (polyX[j] - polyX[i]) * (y - polyY[i]) / (polyY[j] - polyY[i]) + polyX[i]))
                {
                    c = !c;
                }
            }

            return c;
        }
    }
}
