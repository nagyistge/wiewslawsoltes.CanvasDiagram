// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using RxCanvas.Interfaces;

namespace RxCanvas.Droid
{
    public class DroidConverter : INativeConverter
    {
        public IPin Convert(IPin pin)
        {
            return pin;
        }

        public ILine Convert(ILine line)
        {
            return line;
        }

        public IBezier Convert(IBezier bezier)
        {
            return bezier;
        }

        public IQuadraticBezier Convert(IQuadraticBezier quadraticBezier)
        {
            return quadraticBezier;
        }

        public IArc Convert(IArc arc)
        {
            return arc;
        }

        public IRectangle Convert(IRectangle rectangle)
        {
            return rectangle;
        }

        public IEllipse Convert(IEllipse ellipse)
        {
            return ellipse;
        }

        public IText Convert(IText text)
        {
            return text;
        }

        public IBlock Convert(IBlock block)
        {
            return block;
        }

        public ICanvas Convert(ICanvas canvas)
        {
            return new DroidCanvas(canvas);
        }
    }
}
