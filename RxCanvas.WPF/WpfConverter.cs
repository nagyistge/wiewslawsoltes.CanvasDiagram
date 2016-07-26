// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfConverter : INativeConverter
    {
        public IPin Convert(IPin pin)
        {
            return new WpfPin(pin);
        }

        public ILine Convert(ILine line)
        {
            return new WpfLine(line);
        }

        public IBezier Convert(IBezier bezier)
        {
            return new WpfBezier(bezier);
        }

        public IQuadraticBezier Convert(IQuadraticBezier quadraticBezier)
        {
            return new WpfQuadraticBezier(quadraticBezier);
        }

        public IArc Convert(IArc arc)
        {
            return new WpfArc(arc);
        }

        public IRectangle Convert(IRectangle rectangle)
        {
            return new WpfRectangle(rectangle);
        }

        public IEllipse Convert(IEllipse ellipse)
        {
            return new WpfEllipse(ellipse);
        }

        public IText Convert(IText text)
        {
            return new WpfText(text);
        }

        public IBlock Convert(IBlock block)
        {
            return block;
        }

        public ICanvas Convert(ICanvas canvas)
        {
            return new WpfCanvas(canvas);
        }
    }
}
