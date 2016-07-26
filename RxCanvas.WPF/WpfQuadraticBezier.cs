// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfQuadraticBezier : IQuadraticBezier
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _fillBrush;
        private SolidColorBrush _strokeBrush;
        private Path _path;
        private PathGeometry _pg;
        private PathFigure _pf;
        private QuadraticBezierSegment _qbs;
        private IQuadraticBezier _xqb;

        public WpfQuadraticBezier(IQuadraticBezier qb)
        {
            _xqb = qb;

            _fillBrush = new SolidColorBrush(_xqb.Fill.ToNativeColor());
            _fillBrush.Freeze();
            _strokeBrush = new SolidColorBrush(_xqb.Stroke.ToNativeColor());
            _strokeBrush.Freeze();

            _path = new Path();
            _path.Tag = this;
            _path.Fill = _fillBrush;
            _path.Stroke = _strokeBrush;
            _path.StrokeThickness = qb.StrokeThickness;
            _pg = new PathGeometry();
            _pf = new PathFigure();
            _pf.StartPoint = new Point(qb.Start.X, qb.Start.Y);
            _pf.IsFilled = qb.IsFilled;
            _pf.IsClosed = qb.IsClosed;
            _qbs = new QuadraticBezierSegment();
            _qbs.Point1 = new Point(qb.Point1.X, qb.Point1.Y);
            _qbs.Point2 = new Point(qb.Point2.X, qb.Point2.Y);
            _pf.Segments.Add(_qbs);
            _pg.Figures.Add(_pf);
            _path.Data = _pg;

            Native = _path;
        }

        public int Id
        {
            get { return _xqb.Id; }
            set { _xqb.Id = value; }
        }

        public IPoint Start
        {
            get { return _xqb.Start; }
            set
            {
                _xqb.Start = value;
                _pf.StartPoint = new Point(_xqb.Start.X, _xqb.Start.Y);
            }
        }

        public IPoint Point1
        {
            get { return _xqb.Point1; }
            set
            {
                _xqb.Point1 = value;
                _qbs.Point1 = new Point(_xqb.Point1.X, _xqb.Point1.Y);
            }
        }

        public IPoint Point2
        {
            get { return _xqb.Point2; }
            set
            {
                _xqb.Point2 = value;
                _qbs.Point2 = new Point(_xqb.Point2.X, _xqb.Point2.Y);
            }
        }

        public IColor Fill
        {
            get { return _xqb.Fill; }
            set
            {
                _xqb.Fill = value;
                _fillBrush = new SolidColorBrush(_xqb.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _path.Fill = _fillBrush;
            }
        }

        public IColor Stroke
        {
            get { return _xqb.Stroke; }
            set
            {
                _xqb.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xqb.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _path.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xqb.StrokeThickness; }
            set
            {
                _xqb.StrokeThickness = value;
                _path.StrokeThickness = value;
            }
        }

        public bool IsFilled
        {
            get { return _xqb.IsFilled; }
            set
            {
                _xqb.IsFilled = value;
                _pf.IsFilled = value;
            }
        }

        public bool IsClosed
        {
            get { return _xqb.IsClosed; }
            set
            {
                _xqb.IsClosed = value;
                _pf.IsClosed = value;
            }
        }
    }
}
