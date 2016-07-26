// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using MathUtil;
using RxCanvas.Interfaces;

namespace RxCanvas.Model
{
    public static class XModelExtenstion
    {
        private static char[] Separators = new char[] { ',' };

        public static IColor FromHtml(this string str)
        {
            return new XColor(byte.Parse(str.Substring(1, 2), NumberStyles.HexNumber),
                byte.Parse(str.Substring(3, 2), NumberStyles.HexNumber),
                byte.Parse(str.Substring(5, 2), NumberStyles.HexNumber),
                byte.Parse(str.Substring(7, 2), NumberStyles.HexNumber));
        }

        public static string ToHtml(this IColor color)
        {
            return string.Concat('#',
                color.A.ToString("X2"),
                color.R.ToString("X2"),
                color.G.ToString("X2"),
                color.B.ToString("X2"));
        }

        private static NumberFormatInfo NumberFormat = new CultureInfo("en-GB").NumberFormat;

        public static string ToText(this IPoint point)
        {
            
            return string.Concat(
                point.X.ToString(NumberFormat),
                Separators[0],
                point.Y.ToString(NumberFormat));
        }

        public static IPoint FromText(this string str)
        {
            string[] values = str.Split(Separators);
            return new XPoint(
                double.Parse(values[0], NumberFormat),
                double.Parse(values[1], NumberFormat));
        }
    }

    public class XColor : IColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public XColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
    }

    public class XPolygon : IPolygon
    {
        public IPoint[] Points { get; set; }
        public ILine[] Lines { get; set; }

        public bool Contains(IPoint point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(double x, double y)
        {
            bool contains = false;
            for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
            {
                if (((Points[i].Y > y) != (Points[j].Y > y))
                    && (x < (Points[j].X - Points[i].X) * (y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }
    }

    public abstract class XNative : INative
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
    }

    public class XPoint : IPoint, IComparable<XPoint>
    {
        public int Id { get; set; }
        public IList<INative> Connected { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public XPoint(double x, double y)
        {
            Connected = new List<INative>();
            X = x;
            Y = y;
        }

        public static bool operator <(XPoint p1, XPoint p2)
        {
            return p1.X < p2.X || (p1.X == p2.X && p1.Y < p2.Y);
        }

        public static bool operator >(XPoint p1, XPoint p2)
        {
            return p1.X > p2.X || (p1.X == p2.X && p1.Y > p2.Y);
        }

        public int CompareTo(XPoint other)
        {
            return (this > other) ? -1 : ((this < other) ? 1 : 0);
        }
    }

    public class XPin : IPin
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point { get; set; }
        public INative Shape { get; set; }
    }

    public class XLine : ILine
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
    }

    public class XBezier : IBezier
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Start { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public IPoint Point3 { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public IColor Fill { get; set; }
        public bool IsFilled { get; set; }
        public bool IsClosed { get; set; }
    }

    public class XQuadraticBezier : IQuadraticBezier
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Start { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public IColor Fill { get; set; }
        public bool IsFilled { get; set; }
        public bool IsClosed { get; set; }
    }

    public class XArc : IArc
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public double StartAngle { get; set; }
        public double SweepAngle { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public IColor Fill { get; set; }
        public bool IsFilled { get; set; }
        public bool IsClosed { get; set; }
    }

    public class XRectangle : IRectangle
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public IColor Fill { get; set; }
    }

    public class XEllipse : IEllipse
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public IColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public IColor Fill { get; set; }
    }

    public class XText : IText
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }
        public int HorizontalAlignment { get; set; }
        public int VerticalAlignment { get; set; }
        public double Size { get; set; }
        public string Text { get; set; }
        public IColor Foreground { get; set; }
        public IColor Backgroud { get; set; }
    }

    public class XBlock : IBlock
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IList<INative> Children { get; set; }

        public XBlock()
        {
            Children = new List<INative>();
        }
    }

    public class XCanvas : ICanvas
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IObservable<Vector2> Downs { get; set; }
        public IObservable<Vector2> Ups { get; set; }
        public IObservable<Vector2> Moves { get; set; }
        public IHistory History { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IColor Background { get; set; }
        public bool EnableSnap { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public bool IsCaptured { get; set; }
        public IList<INative> Children { get; set; }

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public XCanvas()
        {
            Children = new ObservableCollection<INative>();
        }

        public void Capture()
        {
            IsCaptured = true;
        }

        public void ReleaseCapture()
        {
            IsCaptured = false;
        }

        public void Add(INative value)
        {
            Children.Add(value);
        }

        public void Remove(INative value)
        {
            Children.Remove(value);
        }

        public void Clear()
        {
            Children.Clear();
        }

        public void Render(INative context)
        {
        }
    }

    public class XModelConverter : IModelConverter
    {
        private IColor Convert(IColor color)
        {
            return new XColor(color.A, color.R, color.G, color.B);
        }

        private IPoint Convert(IPoint point)
        {
            return new XPoint(point.X, point.Y);
        }

        public IPin Convert(IPin pin)
        {
            return new XPin()
            {
                Point = Convert(pin.Point),
                Shape = Convert(pin.Shape),
            };
        }

        public ILine Convert(ILine line)
        {
            return new XLine()
            {
                Point1 = Convert(line.Point1),
                Point2 = Convert(line.Point2),
                Stroke = Convert(line.Stroke),
                StrokeThickness = line.StrokeThickness
            };
        }

        public IBezier Convert(IBezier bezier)
        {
            return new XBezier()
            {
                Start = Convert(bezier.Start),
                Point1 = Convert(bezier.Point1),
                Point2 = Convert(bezier.Point2),
                Point3 = Convert(bezier.Point3),
                Stroke = Convert(bezier.Stroke),
                StrokeThickness = bezier.StrokeThickness,
                Fill = Convert(bezier.Fill),
                IsFilled = bezier.IsFilled,
                IsClosed = bezier.IsClosed
            };
        }

        public IQuadraticBezier Convert(IQuadraticBezier quadraticBezier)
        {
            return new XQuadraticBezier()
            {
                Start = Convert(quadraticBezier.Start),
                Point1 = Convert(quadraticBezier.Point1),
                Point2 = Convert(quadraticBezier.Point2),
                Stroke = Convert(quadraticBezier.Stroke),
                StrokeThickness = quadraticBezier.StrokeThickness,
                Fill = Convert(quadraticBezier.Fill),
                IsFilled = quadraticBezier.IsFilled,
                IsClosed = quadraticBezier.IsClosed
            };
        }

        public IArc Convert(IArc arc)
        {
            return new XArc()
            {
                Point1 = Convert(arc.Point1),
                Point2 = Convert(arc.Point2),
                StartAngle = arc.StartAngle,
                SweepAngle = arc.SweepAngle,
                Stroke = Convert(arc.Stroke),
                StrokeThickness = arc.StrokeThickness,
                Fill = Convert(arc.Fill),
                IsFilled = arc.IsFilled,
                IsClosed = arc.IsClosed
            };
        }

        public IRectangle Convert(IRectangle rectangle)
        {
            return new XRectangle()
            {
                Point1 = Convert(rectangle.Point1),
                Point2 = Convert(rectangle.Point2),
                Stroke = Convert(rectangle.Stroke),
                StrokeThickness = rectangle.StrokeThickness,
                Fill = Convert(rectangle.Fill),
            };
        }

        public IEllipse Convert(IEllipse ellipse)
        {
            return new XEllipse()
            {
                Point1 = Convert(ellipse.Point1),
                Point2 = Convert(ellipse.Point2),
                Stroke = Convert(ellipse.Stroke),
                StrokeThickness = ellipse.StrokeThickness,
                Fill = Convert(ellipse.Fill),
            };
        }

        public IText Convert(IText text)
        {
            return new XText()
            {
                Point1 = Convert(text.Point1),
                Point2 = Convert(text.Point2),
                HorizontalAlignment = text.HorizontalAlignment,
                VerticalAlignment = text.VerticalAlignment,
                Size = text.Size,
                Text = text.Text,
                Backgroud = Convert(text.Backgroud),
                Foreground = Convert(text.Foreground)
            };
        }

        public IBlock Convert(IBlock block)
        {
            throw new NotImplementedException();
        }

        public INative Convert(INative native)
        {
            if (native is IPin)
            {
                return Convert(native as IPin);
            }
            else if (native is ILine)
            {
                return Convert(native as ILine);
            }
            else if (native is IBezier)
            {
                return Convert(native as IBezier);
            }
            else if (native is IQuadraticBezier)
            {
                return Convert(native as IQuadraticBezier);
            }
            else if (native is IArc)
            {
                return Convert(native as IArc);
            }
            else if (native is IRectangle)
            {
                return Convert(native as IRectangle);
            }
            else if (native is IEllipse)
            {
                return Convert(native as IEllipse);
            }
            else if (native is IText)
            {
                return Convert(native as IText);
            }
            else if (native is IBlock)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public ICanvas Convert(ICanvas canvas)
        {
            var xcanvas = new XCanvas()
            {
                Width = canvas.Width,
                Height = canvas.Height,
                Background = Convert(canvas.Background),
                SnapX = canvas.SnapX,
                SnapY = canvas.SnapY,
                EnableSnap = canvas.EnableSnap
            };
            var children = new List<INative>();
            foreach (var child in canvas.Children)
            {
                children.Add(Convert(child));
            }
            xcanvas.Children = children;
            return xcanvas;
        }
    }
}
