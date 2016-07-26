// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Android.Graphics;

namespace RxCanvas.Droid
{
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
}
