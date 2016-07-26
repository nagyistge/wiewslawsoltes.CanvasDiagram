// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
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
}
