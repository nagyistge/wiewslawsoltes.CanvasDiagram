// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfLine : ILine
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private Line _nline;
        private ILine _xline;

        public WpfLine(ILine line)
        {
            _xline = line;

            _strokeBrush = new SolidColorBrush(_xline.Stroke.ToNativeColor());
            _strokeBrush.Freeze();

            _nline = new Line()
            {
                X1 = _xline.Point1.X,
                Y1 = _xline.Point1.Y,
                X2 = _xline.Point2.X,
                Y2 = _xline.Point2.Y,
                Stroke = _strokeBrush,
                StrokeThickness = line.StrokeThickness
            };

            Native = _nline;
        }

        public int Id
        {
            get { return _xline.Id; }
            set { _xline.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xline.Point1; }
            set
            {
                _xline.Point1 = value;
                _nline.X1 = _xline.Point1.X;
                _nline.Y1 = _xline.Point1.Y;
            }
        }

        public IPoint Point2
        {
            get { return _xline.Point2; }
            set
            {
                _xline.Point2 = value;
                _nline.X2 = _xline.Point2.X;
                _nline.Y2 = _xline.Point2.Y;
            }
        }

        public IColor Stroke
        {
            get { return _xline.Stroke; }
            set
            {
                _xline.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xline.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _nline.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xline.StrokeThickness; }
            set
            {
                _xline.StrokeThickness = value;
                _nline.StrokeThickness = value;
            }
        }
    }
}
