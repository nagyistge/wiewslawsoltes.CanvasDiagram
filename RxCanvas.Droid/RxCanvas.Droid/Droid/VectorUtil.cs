// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Android.Graphics;

namespace RxCanvas.Droid
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
}
