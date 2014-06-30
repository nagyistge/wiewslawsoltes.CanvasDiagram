using System;

namespace CanvasDiagram.Droid
{
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
