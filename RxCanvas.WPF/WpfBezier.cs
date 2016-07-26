// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfBezier : IBezier
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _fillBrush;
        private SolidColorBrush _strokeBrush;
        private Path _path;
        private PathGeometry _pg;
        private PathFigure _pf;
        private BezierSegment _bs;
        private IBezier _xb;

        public WpfBezier(IBezier b)
        {
            _xb = b;

            _fillBrush = new SolidColorBrush(_xb.Fill.ToNativeColor());
            _fillBrush.Freeze();
            _strokeBrush = new SolidColorBrush(_xb.Stroke.ToNativeColor());
            _strokeBrush.Freeze();

            _path = new Path();
            _path.Tag = this;
            _path.Fill = _fillBrush;
            _path.Stroke = _strokeBrush;
            _path.StrokeThickness = b.StrokeThickness;
            _pg = new PathGeometry();
            _pf = new PathFigure();
            _pf.StartPoint = new Point(b.Start.X, b.Start.Y);
            _pf.IsFilled = b.IsFilled;
            _pf.IsClosed = b.IsClosed;
            _bs = new BezierSegment();
            _bs.Point1 = new Point(b.Point1.X, b.Point1.Y);
            _bs.Point2 = new Point(b.Point2.X, b.Point2.Y);
            _bs.Point3 = new Point(b.Point3.X, b.Point3.Y);
            _pf.Segments.Add(_bs);
            _pg.Figures.Add(_pf);
            _path.Data = _pg;

            Native = _path;
        }

        public int Id
        {
            get { return _xb.Id; }
            set { _xb.Id = value; }
        }

        public IPoint Start
        {
            get { return _xb.Start; }
            set
            {
                _xb.Start = value;
                _pf.StartPoint = new Point(_xb.Start.X, _xb.Start.Y);
            }
        }

        public IPoint Point1
        {
            get { return _xb.Point1; }
            set
            {
                _xb.Point1 = value;
                _bs.Point1 = new Point(_xb.Point1.X, _xb.Point1.Y);
            }
        }

        public IPoint Point2
        {
            get { return _xb.Point2; }
            set
            {
                _xb.Point2 = value;
                _bs.Point2 = new Point(_xb.Point2.X, _xb.Point2.Y);
            }
        }

        public IPoint Point3
        {
            get { return _xb.Point3; }
            set
            {
                _xb.Point3 = value;
                _bs.Point3 = new Point(_xb.Point3.X, _xb.Point3.Y);
            }
        }

        public IColor Fill
        {
            get { return _xb.Fill; }
            set
            {
                _xb.Fill = value;
                _fillBrush = new SolidColorBrush(_xb.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _path.Fill = _fillBrush;
            }
        }

        public IColor Stroke
        {
            get { return _xb.Stroke; }
            set
            {
                _xb.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xb.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _path.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xb.StrokeThickness; }
            set
            {
                _xb.StrokeThickness = value;
                _path.StrokeThickness = value;
            }
        }

        public bool IsFilled
        {
            get { return _xb.IsFilled; }
            set
            {
                _xb.IsFilled = value;
                _pf.IsFilled = value;
            }
        }

        public bool IsClosed
        {
            get { return _xb.IsClosed; }
            set
            {
                _xb.IsClosed = value;
                _pf.IsClosed = value;
            }
        }
    }
}
