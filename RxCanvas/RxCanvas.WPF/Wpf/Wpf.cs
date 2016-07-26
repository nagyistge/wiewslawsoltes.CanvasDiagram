// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using MathUtil;
using RxCanvas.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RxCanvas.WPF
{
    internal static class WpfExtensions
    {
        public static Color ToNativeColor(this IColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }

    public class WpfPin : IPin
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Ellipse _nellipse;
        private double _size;
        private IPin _xpin;

        public WpfPin(IPin pin)
        {
            _xpin = pin;

            _size = 8.0;

            _strokeBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            _fillBrush.Freeze();

            _nellipse = new Ellipse()
            {
                Stroke = _strokeBrush,
                StrokeThickness = 2.0,
                Fill = _fillBrush
            };

            Update();

            Native = _nellipse;
        }

        public int Id
        {
            get { return _xpin.Id; }
            set { _xpin.Id = value; }
        }

        public INative Shape
        {
            get { return _xpin.Shape; }
            set
            {
                _xpin.Shape = value;
                Update();
            }
        }

        public IPoint Point
        {
            get { return _xpin.Point; }
            set
            {
                _xpin.Point = value;
                Update();
            }
        }

        private void Update()
        {
            double hsize = _size / 2.0;
            Canvas.SetLeft(_nellipse, _xpin.Point.X - hsize);
            Canvas.SetTop(_nellipse, _xpin.Point.Y - hsize);
            _nellipse.Width = _size;
            _nellipse.Height = _size;
        }
    }

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

    public class WpfArc : IArc
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _fillBrush;
        private SolidColorBrush _strokeBrush;
        private Path _path;
        private PathGeometry _pg;
        private PathFigure _pf;
        private ArcSegment _as;
        private Point _start;
        private IArc _xarc;

        public WpfArc(IArc arc)
        {
            _xarc = arc;

            _fillBrush = new SolidColorBrush(_xarc.Fill.ToNativeColor());
            _fillBrush.Freeze();
            _strokeBrush = new SolidColorBrush(_xarc.Stroke.ToNativeColor());
            _strokeBrush.Freeze();

            _path = new Path();
            _path.Tag = this;
            _path.Fill = _fillBrush;
            _path.Stroke = _strokeBrush;
            _path.StrokeThickness = arc.StrokeThickness;
            _pg = new PathGeometry();
            _pf = new PathFigure();
            _pf.IsFilled = arc.IsFilled;
            _pf.IsClosed = arc.IsClosed;
            _start = new Point();
            _as = new ArcSegment();
            SetArcSegment(_as, arc, out _start);
            _pf.StartPoint = _start;
            _pf.Segments.Add(_as);
            _pg.Figures.Add(_pf);
            _path.Data = _pg;

            Native = _path;
        }

        public const double Deg2Rad = Math.PI / 180;
        public const double πHalf = Math.PI / 2;

        private void SetArcSegment(
            ArcSegment segment, 
            IArc arc, 
            out Point startPoint)
        {
            // original code https://pdfsharp.codeplex.com/
            // PDFsharp/code/PdfSharp/PdfSharp.Internal/Calc.cs
            // PDFsharp/code/PdfSharp/PdfSharp.Drawing/GeometryHelper.cs

            double x = Math.Min(arc.Point1.X, arc.Point2.X);
            double y = Math.Min(arc.Point1.Y, arc.Point2.Y);
            double width = Math.Abs(arc.Point2.X - arc.Point1.X);
            double height = Math.Abs(arc.Point2.Y - arc.Point1.Y);
            double startAngle = arc.StartAngle;
            double sweepAngle = arc.SweepAngle;

            // normalize the angles
            double α = startAngle;
            if (α < 0)
            {
                α = α + (1 + Math.Floor((Math.Abs(α) / 360))) * 360;
            }
            else if (α > 360)
            {
                α = α - Math.Floor(α / 360) * 360;
            }

            Debug.Assert(α >= 0 && α <= 360);

            if (Math.Abs(sweepAngle) >= 360)
            {
                sweepAngle = Math.Sign(sweepAngle) * 360;
            }

            double β = startAngle + sweepAngle;
            if (β < 0)
            {
                β = β + (1 + Math.Floor((Math.Abs(β) / 360))) * 360;
            }
            else if (β > 360)
            {
                β = β - Math.Floor(β / 360) * 360;
            }

            if (α == 0 && β < 0)
            {
                α = 360;
            }
            else if (α == 360 && β > 0)
            {
                α = 0;
            }

            // scanling factor
            double δx = width / 2;
            double δy = height / 2;
            // center of ellipse
            double x0 = x + δx;
            double y0 = y + δy;
            double cosα, cosβ, sinα, sinβ;

            if (width == height)
            {
                // circular arc needs no correction
                α = α * Deg2Rad;
                β = β * Deg2Rad;
            }
            else
            {
                // elliptic arc needs the angles to be adjusted 
                // such that the scaling transformation is compensated
                α = α * Deg2Rad;
                sinα = Math.Sin(α);
                if (Math.Abs(sinα) > 1E-10)
                {
                    if (α < Math.PI)
                    {
                        α = Math.PI / 2 - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
                    }
                    else
                    {
                        α = 3 * Math.PI / 2 - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
                    }
                }
                // α = πHalf - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
                β = β * Deg2Rad;
                sinβ = Math.Sin(β);
                if (Math.Abs(sinβ) > 1E-10)
                {
                    if (β < Math.PI)
                    {
                        β = Math.PI / 2 - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
                    }
                    else
                    {
                        β = 3 * Math.PI / 2 - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
                    }
                }
                // β = πHalf - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
            }

            sinα = Math.Sin(α);
            cosα = Math.Cos(α);
            sinβ = Math.Sin(β);
            cosβ = Math.Cos(β);

            startPoint = new Point(x0 + δx * cosα, y0 + δy * sinα);
            var destPoint = new Point(x0 + δx * cosβ, y0 + δy * sinβ);
            var size = new Size(δx, δy);
            bool isLargeArc = Math.Abs(sweepAngle) >= 180;
            SweepDirection sweepDirection = 
                sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
            bool isStroked = true;

            segment.Point = destPoint;
            segment.Size = size;
            segment.RotationAngle = 0.0;
            segment.IsLargeArc = isLargeArc;
            segment.SweepDirection = sweepDirection;
            segment.IsStroked = isStroked;
        }

        public int Id
        {
            get { return _xarc.Id; }
            set { _xarc.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xarc.Point1; }
            set
            {
                _xarc.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xarc.Point2; }
            set
            {
                _xarc.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            SetArcSegment(_as, _xarc, out _start);
            _pf.StartPoint = _start;
        }

        public double StartAngle
        {
            get { return _xarc.StartAngle; }
            set 
            {
                _xarc.StartAngle = value;
                SetArcSegment(_as, _xarc, out _start);
                _pf.StartPoint = _start;
            }
        }

        public double SweepAngle
        {
            get { return _xarc.SweepAngle; }
            set 
            {
                _xarc.SweepAngle = value;
                SetArcSegment(_as, _xarc, out _start);
                _pf.StartPoint = _start;
            }
        }

        public IColor Stroke
        {
            get { return _xarc.Stroke; }
            set
            {
                _xarc.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xarc.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _path.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xarc.StrokeThickness; }
            set 
            {
                _xarc.StrokeThickness = value;
                _path.StrokeThickness = value; 
            }
        }

        public IColor Fill
        {
            get { return _xarc.Fill; }
            set
            {
                _xarc.Fill = value;
                _fillBrush = new SolidColorBrush(_xarc.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _path.Fill = _fillBrush;
            }
        }

        public bool IsFilled
        {
            get { return _xarc.IsFilled; }
            set 
            {
                _xarc.IsFilled = value;
                _pf.IsFilled = value; 
            }
        }

        public bool IsClosed
        {
            get { return _xarc.IsClosed; }
            set 
            {
                _xarc.IsClosed = value;
                _pf.IsClosed = value; 
            }
        }
    }

    public class WpfRectangle : IRectangle
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Rectangle _nrectangle;
        private IRectangle _xrectangle;

        public WpfRectangle(IRectangle rectangle)
        {
            _xrectangle = rectangle;

            _strokeBrush = new SolidColorBrush(_xrectangle.Stroke.ToNativeColor());
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(_xrectangle.Fill.ToNativeColor());
            _fillBrush.Freeze();

            _nrectangle = new Rectangle()
            {
                Stroke = _strokeBrush,
                StrokeThickness = rectangle.StrokeThickness,
                Fill = _fillBrush
            };

            Update();

            Native = _nrectangle;
        }

        public int Id
        {
            get { return _xrectangle.Id; }
            set { _xrectangle.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xrectangle.Point1; }
            set
            {
                _xrectangle.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xrectangle.Point2; }
            set
            {
                _xrectangle.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xrectangle.Point1.X, _xrectangle.Point2.X);
            double y = Math.Min(_xrectangle.Point1.Y, _xrectangle.Point2.Y);
            double width = Math.Abs(_xrectangle.Point2.X - _xrectangle.Point1.X);
            double height = Math.Abs(_xrectangle.Point2.Y - _xrectangle.Point1.Y);
            Canvas.SetLeft(_nrectangle, x - 1.0);
            Canvas.SetTop(_nrectangle, y - 1.0);
            _nrectangle.Width = width + 2.0;
            _nrectangle.Height = height + 2.0;
        }

        public IColor Stroke
        {
            get { return _xrectangle.Stroke; }
            set
            {
                _xrectangle.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xrectangle.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _nrectangle.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xrectangle.StrokeThickness; }
            set 
            {
                _xrectangle.StrokeThickness = value;
                _nrectangle.StrokeThickness = value; 
            }
        }

        public IColor Fill
        {
            get { return _xrectangle.Fill; }
            set
            {
                _xrectangle.Fill = value;
                _fillBrush = new SolidColorBrush(_xrectangle.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _nrectangle.Fill = _fillBrush;
            }
        }
    }

    public class WpfEllipse : IEllipse
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Ellipse _nellipse;
        private IEllipse _xellipse;

        public WpfEllipse(IEllipse ellipse)
        {
            _xellipse = ellipse;

            _strokeBrush = new SolidColorBrush(_xellipse.Stroke.ToNativeColor());
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(_xellipse.Fill.ToNativeColor());
            _fillBrush.Freeze();

            _nellipse = new Ellipse()
            {
                Stroke = _strokeBrush,
                StrokeThickness = ellipse.StrokeThickness,
                Fill = _fillBrush
            };

            Update();

            Native = _nellipse;
        }

        public int Id
        {
            get { return _xellipse.Id; }
            set { _xellipse.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xellipse.Point1; }
            set
            {
                _xellipse.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xellipse.Point2; }
            set
            {
                _xellipse.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xellipse.Point1.X, _xellipse.Point2.X);
            double y = Math.Min(_xellipse.Point1.Y, _xellipse.Point2.Y);
            double width = Math.Abs(_xellipse.Point2.X - _xellipse.Point1.X);
            double height = Math.Abs(_xellipse.Point2.Y - _xellipse.Point1.Y);
            Canvas.SetLeft(_nellipse, x - 1.0);
            Canvas.SetTop(_nellipse, y - 1.0);
            _nellipse.Width = width + 2.0;
            _nellipse.Height = height + 2.0;
        }

        public IColor Stroke
        {
            get { return _xellipse.Stroke; }
            set
            {
                _xellipse.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xellipse.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _nellipse.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xellipse.StrokeThickness; }
            set 
            {
                _xellipse.StrokeThickness = value;
                _nellipse.StrokeThickness = value; 
            }
        }

        public IColor Fill
        {
            get { return _xellipse.Fill; }
            set
            {
                _xellipse.Fill = value;
                _fillBrush = new SolidColorBrush(_xellipse.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _nellipse.Fill = _fillBrush;
            }
        }
    }

    public class WpfText : IText
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _foregroundBrush;
        private SolidColorBrush _backgroundBrush;
        private Grid _grid;
        private TextBlock _tb;
        private IText _xtext;

        public WpfText(IText text)
        {
            _xtext = text;

            _foregroundBrush = new SolidColorBrush(_xtext.Foreground.ToNativeColor());
            _foregroundBrush.Freeze();
            _backgroundBrush = new SolidColorBrush(_xtext.Backgroud.ToNativeColor());
            _backgroundBrush.Freeze();

            _grid = new Grid();
            _grid.Background = _backgroundBrush;

            _tb = new TextBlock();
            _tb.HorizontalAlignment = (HorizontalAlignment)text.HorizontalAlignment;
            _tb.VerticalAlignment = (VerticalAlignment)text.VerticalAlignment;
            _tb.Background = _backgroundBrush;
            _tb.Foreground = _foregroundBrush;
            _tb.FontSize = text.Size;
            _tb.FontFamily = new FontFamily("Calibri");
            _tb.Text = text.Text;

            _grid.Children.Add(_tb);

            Update();

            Native = _grid;
        }

        public int Id
        {
            get { return _xtext.Id; }
            set { _xtext.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xtext.Point1; }
            set
            {
                _xtext.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xtext.Point2; }
            set
            {
                _xtext.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xtext.Point1.X, _xtext.Point2.X);
            double y = Math.Min(_xtext.Point1.Y, _xtext.Point2.Y);
            double width = Math.Abs(_xtext.Point2.X - _xtext.Point1.X);
            double height = Math.Abs(_xtext.Point2.Y - _xtext.Point1.Y);
            Canvas.SetLeft(_grid, x - 1.0);
            Canvas.SetTop(_grid, y - 1.0);
            _grid.Width = width + 2.0;
            _grid.Height = height + 2.0;
        }

        public int HorizontalAlignment
        {
            get { return _xtext.HorizontalAlignment; }
            set 
            {
                _xtext.HorizontalAlignment = value;
                _tb.HorizontalAlignment = (HorizontalAlignment)value; 
            }
        }

        public int VerticalAlignment
        {
            get { return _xtext.VerticalAlignment; }
            set 
            {
                _xtext.VerticalAlignment = value;
                _tb.VerticalAlignment = (VerticalAlignment)value; 
            }
        }

        public double Size
        {
            get { return _xtext.Size; }
            set 
            {
                _xtext.Size = value;
                _tb.FontSize = value; 
            }
        }

        public string Text
        {
            get { return _xtext.Text; }
            set 
            {
                _xtext.Text = value;
                _tb.Text = value; 
            }
        }

        public IColor Foreground
        {
            get { return _xtext.Foreground; }
            set
            {
                _xtext.Foreground = value;
                _foregroundBrush = new SolidColorBrush(_xtext.Foreground.ToNativeColor());
                _foregroundBrush.Freeze();
                _tb.Foreground = _foregroundBrush;
            }
        }

        public IColor Backgroud
        {
            get { return _xtext.Backgroud; }
            set
            {
                _xtext.Backgroud = value;
                _backgroundBrush = new SolidColorBrush(_xtext.Backgroud.ToNativeColor());
                _backgroundBrush.Freeze();
                _grid.Background = _backgroundBrush;
                _tb.Background = _backgroundBrush;
            }
        }
    }

    public class WpfCanvas : ICanvas
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IObservable<Vector2> Downs { get; set; }
        public IObservable<Vector2> Ups { get; set; }
        public IObservable<Vector2> Moves { get; set; }
        private SolidColorBrush _backgroundBrush;
        private ICanvas _xcanvas;
        private Canvas _ncanvas;
    
        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public WpfCanvas(ICanvas canvas)
        {
            _xcanvas = canvas;

            _backgroundBrush = new SolidColorBrush(_xcanvas.Background.ToNativeColor());
            _backgroundBrush.Freeze();

            _ncanvas = new Canvas()
            {
                Width = canvas.Width,
                Height = canvas.Height,
                Background = _backgroundBrush
            };

            Downs = Observable.FromEventPattern<MouseButtonEventArgs>(
                _ncanvas, 
                "PreviewMouseLeftButtonDown").Select(e =>
            {
                var p = e.EventArgs.GetPosition(_ncanvas);
                return new Vector2(
                    _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                    _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
            });

            Ups = Observable.FromEventPattern<MouseButtonEventArgs>(
                _ncanvas, 
                "PreviewMouseLeftButtonUp").Select(e =>
            {
                var p = e.EventArgs.GetPosition(_ncanvas);
                return new Vector2(
                    _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                    _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
            });

            Moves = Observable.FromEventPattern<MouseEventArgs>(
                _ncanvas, 
                "PreviewMouseMove").Select(e =>
            {
                var p = e.EventArgs.GetPosition(_ncanvas);
                return new Vector2(
                    _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                    _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
            });

            Native = _ncanvas;
        }

        public int Id
        {
            get { return _xcanvas.Id; }
            set { _xcanvas.Id = value; }
        }

        public IHistory History
        {
            get { return _xcanvas.History; }
            set { _xcanvas.History = value; }
        }

        public IList<INative> Children
        {
            get { return _xcanvas.Children; }
            set { _xcanvas.Children = value; }
        }

        public double Width
        {
            get { return _ncanvas.Width; }
            set { _ncanvas.Width = value; }
        }

        public double Height
        {
            get { return _ncanvas.Height; }
            set { _ncanvas.Height = value; }
        }

        public IColor Background
        {
            get { return _xcanvas.Background; }
            set
            {
                _xcanvas.Background = value;
                if (_xcanvas.Background == null)
                {
                    _backgroundBrush = null;
                }
                else
                {
                    _backgroundBrush = new SolidColorBrush(_xcanvas.Background.ToNativeColor());
                    _backgroundBrush.Freeze();
                }
                _ncanvas.Background = _backgroundBrush;
            }
        }

        public bool EnableSnap
        {
            get { return _xcanvas.EnableSnap; }
            set { _xcanvas.EnableSnap = value; }
        }

        public double SnapX
        {
            get { return _xcanvas.SnapX; }
            set { _xcanvas.SnapX = value; }
        }

        public double SnapY
        {
            get { return _xcanvas.SnapY; }
            set { _xcanvas.SnapY = value; }
        }

        public bool IsCaptured
        {
            get { return Mouse.Captured == _ncanvas; }
            set { _ncanvas.CaptureMouse(); }
        }

        public void Capture()
        {
            _ncanvas.CaptureMouse();
        }

        public void ReleaseCapture()
        {
            _ncanvas.ReleaseMouseCapture();
        }

        public void Add(INative value)
        {
            if (value.Native != null)
            {
                _ncanvas.Children.Add(value.Native as UIElement);
            }

            Children.Add(value);
        }

        public void Remove(INative value)
        {
            if (value.Native != null)
            {
                _ncanvas.Children.Remove(value.Native as UIElement);
            }

            Children.Remove(value);
        }

        public void Clear()
        {
            _ncanvas.Children.Clear();
            Children.Clear();
        }

        public void Render(INative context)
        {
        }
    }

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
