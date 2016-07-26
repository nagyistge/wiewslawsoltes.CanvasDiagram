// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using MathUtil;
using RxCanvas.Interfaces;

namespace RxCanvas.Bounds
{
    internal static class Helper
    {
        public static MonotoneChain ConvexHull = new MonotoneChain();

        public const int PointBoundVertexCount = 4;

        public static double Min(double val1, double val2, double val3, double val4)
        {
            return Math.Min(Math.Min(val1, val2), Math.Min(val3, val4));
        }

        public static double Max(double val1, double val2, double val3, double val4)
        {
            return Math.Max(Math.Max(val1, val2), Math.Max(val3, val4));
        }

        public static IPolygon CreateBoundsPolygon(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            int points)
        {
            var polygon = canvasFactory.CreatePolygon();
            polygon.Points = new IPoint[points];
            polygon.Lines = new ILine[points];

            for (int i = 0; i < points; i++)
            {
                polygon.Points[i] = canvasFactory.CreatePoint();

                var _xline = canvasFactory.CreateLine();
                _xline.Stroke = canvasFactory.CreateColor();
                _xline.Stroke.A = 0xFF;
                _xline.Stroke.R = 0x00;
                _xline.Stroke.G = 0xBF;
                _xline.Stroke.B = 0xFF;
                _xline.StrokeThickness = 2.0;
                var _nline = nativeConverter.Convert(_xline);
                polygon.Lines[i] = _nline;
            }

            return polygon;
        }

        public static void UpdatePointBounds(
            IPoint point, 
            IPoint[] ps, ILine[] ls, 
            double size, double offset)
        {
            Debug.Assert(point != null);

            double x = point.X - (size / 2.0);
            double y = point.Y - (size / 2.0);
            double width = size;
            double height = size;

            Helper.UpdateRectangleBounds(ps, ls, offset, x, y, width, height);
        }

        public static void UpdateRectangleBounds(
            IPoint[] ps, ILine[] ls, 
            double offset, 
            double x, double y, 
            double width, double height)
        {
            Debug.Assert(ps != null);
            Debug.Assert(ls != null);
            Debug.Assert(ps.Length == PointBoundVertexCount);
            Debug.Assert(ls.Length == PointBoundVertexCount);

            // top-left
            ps[0].X = x - offset;
            ps[0].Y = y - offset;
            // top-right
            ps[1].X = (x + width) + offset;
            ps[1].Y = y - offset;
            // botton-right
            ps[2].X = (x + width) + offset;
            ps[2].Y = (y + height) + offset;
            // bottom-left
            ps[3].X = x - offset;
            ps[3].Y = (y + height) + offset;

            Helper.MoveLine(ls[0], ps[0], ps[1]);
            Helper.MoveLine(ls[1], ps[1], ps[2]);
            Helper.MoveLine(ls[2], ps[2], ps[3]);
            Helper.MoveLine(ls[3], ps[3], ps[0]);
        }

        public static void MoveLine(ILine line, IPoint point1, IPoint point2)
        {
            line.Point1 = point1;
            line.Point2 = point2;
        }

        public static void MoveLine(ILine line, Vector2 point1, Vector2 point2)
        {
            line.Point1.X = point1.X;
            line.Point1.Y = point1.Y;
            line.Point2.X = point2.X;
            line.Point2.Y = point2.Y;
            line.Point1 = line.Point1;
            line.Point2 = line.Point2;
        }

        public static void UpdateConnected(IPoint point, double dx, double dy)
        {
            foreach (var connected in point.Connected)
            {
                var bounds = connected.Bounds;
                if (bounds != null)
                {
                    //bounds.MoveAll(dx, dy);
                    bounds.Update();
                }
            }
        }
    }

    public class PinBounds : IBounds
    {
        private IPin _pin;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonPoint;
        private bool _isVisible;

        private enum HitResult { None, Point };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public PinBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IPin pin,
            double size,
            double offset)
        {
            _pin = pin;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePointBounds()
        {
            var ps = _polygonPoint.Points;
            var ls = _polygonPoint.Lines;
            Helper.UpdatePointBounds(_pin.Point, ps, ls, _size, _offset);

            double x = _pin.Point.X - (_size / 2.0);
            double y = _pin.Point.Y - (_size / 2.0);
            double width = _size;
            double height = _size;
            UpdateVertices(x, y, width, height);
        }

        private void UpdateVertices(double x, double y, double width, double height)
        {
            _vertices[0] = new Vector2(x, y);
            _vertices[1] = new Vector2(x + width, y);
            _vertices[2] = new Vector2(x + width, y + height);
            _vertices[3] = new Vector2(x, y + height);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePointBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonPoint.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonPoint.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint.Contains(x, y))
            {
                _hitResult = HitResult.Point;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint.Contains(x, y))
            {
                return _pin.Point;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch(_hitResult)
            {
                case HitResult.Point:
                    MovePoint(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MovePoint(dx, dy);
        }

        private void MovePoint(double dx, double dy)
        {
            double x = _pin.Point.X - dx;
            double y = _pin.Point.Y - dy;
            _pin.Point.X = _canvas.EnableSnap ? _canvas.Snap(x, _canvas.SnapX) : x;
            _pin.Point.Y = _canvas.EnableSnap ? _canvas.Snap(y, _canvas.SnapY) : y;
            _pin.Point = _pin.Point;
            Helper.UpdateConnected(_pin.Point, dx, dy);
        }
    }

    public class LineBounds : IBounds
    {
        private ILine _line;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonLine;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Point1, Point2, Line };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public LineBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            ILine line,
            double size,
            double offset)
        {
            _line = line;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonLine = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_line.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_line.Point2, ps, ls, _size, _offset);
        }

        private void UpdateLineBounds()
        {
            var ps = _polygonLine.Points;
            var ls = _polygonLine.Lines;
            var ps1 = _polygonPoint1.Points;
            var ps2 = _polygonPoint2.Points;

            double min1X = Helper.Min(ps1[0].X, ps1[1].X, ps1[2].X, ps1[3].X);
            double min1Y = Helper.Min(ps1[0].Y, ps1[1].Y, ps1[2].Y, ps1[3].Y);
            double max1X = Helper.Max(ps1[0].X, ps1[1].X, ps1[2].X, ps1[3].X);
            double max1Y = Helper.Max(ps1[0].Y, ps1[1].Y, ps1[2].Y, ps1[3].Y);
            double min2X = Helper.Min(ps2[0].X, ps2[1].X, ps2[2].X, ps2[3].X);
            double min2Y = Helper.Min(ps2[0].Y, ps2[1].Y, ps2[2].Y, ps2[3].Y);
            double max2X = Helper.Max(ps2[0].X, ps2[1].X, ps2[2].X, ps2[3].X);
            double max2Y = Helper.Max(ps2[0].Y, ps2[1].Y, ps2[2].Y, ps2[3].Y);

            if (Math.Round(_line.Point1.X, 1) == Math.Round(_line.Point2.X, 1))
            {
                ps[0].X = Math.Min(min1X, min2X);
                ps[0].Y = Math.Max(min1Y, min2Y);
                ps[1].X = Math.Min(min1X, min2X);
                ps[1].Y = Math.Min(max1Y, max2Y);
                ps[2].X = Math.Max(max1X, max2X);
                ps[2].Y = Math.Min(max1Y, max2Y);
                ps[3].X = Math.Max(max1X, max2X);
                ps[3].Y = Math.Max(min1Y, min2Y);
            }
            else if (Math.Round(_line.Point1.Y, 1) == Math.Round(_line.Point2.Y, 1))
            {
                ps[0].X = Math.Max(min1X, min2X);
                ps[0].Y = Math.Min(min1Y, min2Y);
                ps[1].X = Math.Min(max1X, max2X);
                ps[1].Y = Math.Min(min1Y, min2Y);
                ps[2].X = Math.Min(max1X, max2X);
                ps[2].Y = Math.Max(max1Y, max2Y);
                ps[3].X = Math.Max(min1X, min2X);
                ps[3].Y = Math.Max(max1Y, max2Y);
            }
            else
            {
                if (((_line.Point2.X > _line.Point1.X) && (_line.Point2.Y < _line.Point1.Y)) ||
                    ((_line.Point2.X < _line.Point1.X) && (_line.Point2.Y > _line.Point1.Y)))
                {
                    ps[0].X = min1X;
                    ps[0].Y = min1Y;
                    ps[1].X = max1X;
                    ps[1].Y = max1Y;
                    ps[2].X = max2X;
                    ps[2].Y = max2Y;
                    ps[3].X = min2X;
                    ps[3].Y = min2Y;
                }
                else
                {
                    ps[0].X = min1X;
                    ps[0].Y = max1Y;
                    ps[1].X = max1X;
                    ps[1].Y = min1Y;
                    ps[2].X = max2X;
                    ps[2].Y = min2Y;
                    ps[3].X = min2X;
                    ps[3].Y = max2Y;
                }
            }

            Helper.MoveLine(ls[0], ps[0], ps[1]);
            Helper.MoveLine(ls[1], ps[1], ps[2]);
            Helper.MoveLine(ls[2], ps[2], ps[3]);
            Helper.MoveLine(ls[3], ps[3], ps[0]);

            UpdateVertices(ps);
        }

        private void UpdateVertices(IPoint[] ps)
        {
            _vertices[0] = new Vector2(ps[0].X, ps[0].Y);
            _vertices[1] = new Vector2(ps[1].X, ps[1].Y);
            _vertices[2] = new Vector2(ps[2].X, ps[2].Y);
            _vertices[3] = new Vector2(ps[3].X, ps[3].Y);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateLineBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonLine.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonLine.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonLine.Contains(x, y))
            {
                _hitResult = HitResult.Line;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                return _line.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _line.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch(_hitResult)
            {
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Line:
                    MoveLine(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveLine(dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _line.Point1.X - dx;
            double y1 = _line.Point1.Y - dy;
            _line.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _line.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _line.Point1 = _line.Point1;
            Helper.UpdateConnected(_line.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _line.Point2.X - dx;
            double y2 = _line.Point2.Y - dy;
            _line.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _line.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _line.Point2 = _line.Point2;
            Helper.UpdateConnected(_line.Point2, dx, dy);
        }

        private void MoveLine(double dx, double dy)
        {
            double x1 = _line.Point1.X - dx;
            double y1 = _line.Point1.Y - dy;
            double x2 = _line.Point2.X - dx;
            double y2 = _line.Point2.Y - dy;
            _line.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _line.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _line.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _line.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _line.Point1 = _line.Point1;
            _line.Point2 = _line.Point2;
            Helper.UpdateConnected(_line.Point1, dx, dy);
            Helper.UpdateConnected(_line.Point2, dx, dy);
        }
    }

    public class BezierBounds : IBounds
    {
        private IBezier _bezier;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonBezier;
        private IPolygon _polygonStart;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private IPolygon _polygonPoint3;
        private bool _isVisible;

        private enum HitResult { None, Start, Point1, Point2, Point3, Bezier };
        private HitResult _hitResult;
       
        private Vector2[] _vertices;
        private int k;
        private Vector2[] _convexHull;

        public BezierBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IBezier bezier,
            double size,
            double offset)
        {
            _bezier = bezier;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonStart = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint3 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonBezier = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdateStartBounds()
        {
            var ps = _polygonStart.Points;
            var ls = _polygonStart.Lines;
            Helper.UpdatePointBounds(_bezier.Start, ps, ls, _size, _offset);
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_bezier.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_bezier.Point2, ps, ls, _size, _offset);
        }

        private void UpdatePoint3Bounds()
        {
            var ps = _polygonPoint3.Points;
            var ls = _polygonPoint3.Lines;
            Helper.UpdatePointBounds(_bezier.Point3, ps, ls, _size, _offset);
        }

        public bool ConvexHullAsPolygonContains(double x, double y)
        {
            bool contains = false;
            for (int i = 0, j = k - 2; i < k - 1; j = i++)
            {
                if (((_convexHull[i].Y > y) != (_convexHull[j].Y > y))
                    && (x < (_convexHull[j].X - _convexHull[i].X) * (y - _convexHull[i].Y) / (_convexHull[j].Y - _convexHull[i].Y) + _convexHull[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        private void UpdateBezierBounds()
        {
            var ls = _polygonBezier.Lines;

            _vertices[0] = new Vector2(_bezier.Start.X, _bezier.Start.Y);
            _vertices[1] = new Vector2(_bezier.Point1.X, _bezier.Point1.Y);
            _vertices[2] = new Vector2(_bezier.Point2.X, _bezier.Point2.Y);
            _vertices[3] = new Vector2(_bezier.Point3.X, _bezier.Point3.Y);

            Helper.ConvexHull.ConvexHull(_vertices, out _convexHull, out k);

            //Debug.WriteLine("k: {0}", k);

            if (k == 3)
            {
                Helper.MoveLine(ls[0], _convexHull[0], _convexHull[1]);
                Helper.MoveLine(ls[1], _convexHull[1], _convexHull[2]);

                // not used
                Helper.MoveLine(ls[2], _convexHull[0], _convexHull[0]);
                Helper.MoveLine(ls[3], _convexHull[0], _convexHull[0]);
            }
            else if (k == 4)
            {
                Helper.MoveLine(ls[0], _convexHull[0], _convexHull[1]);
                Helper.MoveLine(ls[1], _convexHull[1], _convexHull[2]);
                Helper.MoveLine(ls[2], _convexHull[2], _convexHull[3]);

                // not used
                Helper.MoveLine(ls[3], _convexHull[0], _convexHull[0]);
            }
            else if (k == 5)
            {
                Helper.MoveLine(ls[0], _convexHull[0], _convexHull[1]);
                Helper.MoveLine(ls[1], _convexHull[1], _convexHull[2]);
                Helper.MoveLine(ls[2], _convexHull[2], _convexHull[3]);
                Helper.MoveLine(ls[3], _convexHull[3], _convexHull[4]);
            }
        }

        public Vector2[] GetVertices()
        {
            return _convexHull.Take(k).ToArray();
        }

        public void Update()
        {
            UpdateStartBounds();
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdatePoint3Bounds();
            UpdateBezierBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonBezier.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonStart.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint3.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonBezier.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonStart.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint3.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonStart.Contains(x, y))
            {
                _hitResult = HitResult.Start;
                return true;
            }
            else if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonPoint3.Contains(x, y))
            {
                _hitResult = HitResult.Point3;
                return true;
            }
            else if (ConvexHullAsPolygonContains(x, y)) //_polygonBezier.Contains(x, y)
            {
                _hitResult = HitResult.Bezier;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonStart.Contains(x, y))
            {
                return _bezier.Start;
            }
            else if (_polygonPoint1.Contains(x, y))
            {
                return _bezier.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _bezier.Point2;
            }
            else if (_polygonPoint3.Contains(x, y))
            {
                return _bezier.Point3;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Start:
                    MoveStart(dx, dy);
                    break;
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Point3:
                    MovePoint3(dx, dy);
                    break;
                case HitResult.Bezier:
                    MoveBezier(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveBezier(dx, dy);
        }

        private void MoveStart(double dx, double dy)
        {
            double x = _bezier.Start.X - dx;
            double y = _bezier.Start.Y - dy;
            _bezier.Start.X = _canvas.EnableSnap ? _canvas.Snap(x, _canvas.SnapX) : x;
            _bezier.Start.Y = _canvas.EnableSnap ? _canvas.Snap(y, _canvas.SnapY) : y;
            _bezier.Start = _bezier.Start;
            Helper.UpdateConnected(_bezier.Start, dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _bezier.Point1.X - dx;
            double y1 = _bezier.Point1.Y - dy;
            _bezier.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _bezier.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _bezier.Point1 = _bezier.Point1;
            Helper.UpdateConnected(_bezier.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _bezier.Point2.X - dx;
            double y2 = _bezier.Point2.Y - dy;
            _bezier.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _bezier.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _bezier.Point2 = _bezier.Point2;
            Helper.UpdateConnected(_bezier.Point2, dx, dy);
        }

        private void MovePoint3(double dx, double dy)
        {
            double x3 = _bezier.Point3.X - dx;
            double y3 = _bezier.Point3.Y - dy;
            _bezier.Point3.X = _canvas.EnableSnap ? _canvas.Snap(x3, _canvas.SnapX) : x3;
            _bezier.Point3.Y = _canvas.EnableSnap ? _canvas.Snap(y3, _canvas.SnapY) : y3;
            _bezier.Point3 = _bezier.Point3;
            Helper.UpdateConnected(_bezier.Point3, dx, dy);
        }

        private void MoveBezier(double dx, double dy)
        {
            double x = _bezier.Start.X - dx;
            double y = _bezier.Start.Y - dy;
            double x1 = _bezier.Point1.X - dx;
            double y1 = _bezier.Point1.Y - dy;
            double x2 = _bezier.Point2.X - dx;
            double y2 = _bezier.Point2.Y - dy;
            double x3 = _bezier.Point3.X - dx;
            double y3 = _bezier.Point3.Y - dy;
            _bezier.Start.X = _canvas.EnableSnap ? _canvas.Snap(x, _canvas.SnapX) : x;
            _bezier.Start.Y = _canvas.EnableSnap ? _canvas.Snap(y, _canvas.SnapY) : y;
            _bezier.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _bezier.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _bezier.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _bezier.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _bezier.Point3.X = _canvas.EnableSnap ? _canvas.Snap(x3, _canvas.SnapX) : x3;
            _bezier.Point3.Y = _canvas.EnableSnap ? _canvas.Snap(y3, _canvas.SnapY) : y3;
            _bezier.Start = _bezier.Start;
            _bezier.Point1 = _bezier.Point1;
            _bezier.Point2 = _bezier.Point2;
            _bezier.Point3 = _bezier.Point3;
            Helper.UpdateConnected(_bezier.Start, dx, dy);
            Helper.UpdateConnected(_bezier.Point1, dx, dy);
            Helper.UpdateConnected(_bezier.Point2, dx, dy);
            Helper.UpdateConnected(_bezier.Point3, dx, dy);
        }
    }

    public class QuadraticBezierBounds : IBounds
    {
        private IQuadraticBezier _quadraticBezier;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonQuadraticBezier;
        private IPolygon _polygonStart;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Start, Point1, Point2, QuadraticBezier };
        private HitResult _hitResult;

        private Vector2[] _vertices;
        private int k;
        private Vector2[] _convexHull;

        public QuadraticBezierBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IQuadraticBezier quadraticBezier,
            double size,
            double offset)
        {
            _quadraticBezier = quadraticBezier;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonStart = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonQuadraticBezier = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 3);
            _vertices = new Vector2[3];
        }

        private void UpdateStartBounds()
        {
            var ps = _polygonStart.Points;
            var ls = _polygonStart.Lines;
            Helper.UpdatePointBounds(_quadraticBezier.Start, ps, ls, _size, _offset);
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_quadraticBezier.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_quadraticBezier.Point2, ps, ls, _size, _offset);
        }

        public bool ConvexHullAsPolygonContains(double x, double y)
        {
            bool contains = false;
            for (int i = 0, j = k - 2; i < k - 1; j = i++)
            {
                if (((_convexHull[i].Y > y) != (_convexHull[j].Y > y))
                    && (x < (_convexHull[j].X - _convexHull[i].X) * (y - _convexHull[i].Y) / (_convexHull[j].Y - _convexHull[i].Y) + _convexHull[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        private void UpdateQuadraticBezierBounds()
        {
            var ls = _polygonQuadraticBezier.Lines;

            _vertices[0] = new Vector2(_quadraticBezier.Start.X, _quadraticBezier.Start.Y);
            _vertices[1] = new Vector2(_quadraticBezier.Point1.X, _quadraticBezier.Point1.Y);
            _vertices[2] = new Vector2(_quadraticBezier.Point2.X, _quadraticBezier.Point2.Y);

            Helper.ConvexHull.ConvexHull(_vertices, out _convexHull, out k);

            //Debug.WriteLine("k: {0}", k);

            if (k == 3)
            {
                Helper.MoveLine(ls[0], _convexHull[0], _convexHull[1]);
                Helper.MoveLine(ls[1], _convexHull[1], _convexHull[2]);

                // not used
                Helper.MoveLine(ls[2], _convexHull[0], _convexHull[0]);
            }
            else if (k == 4)
            {
                Helper.MoveLine(ls[0], _convexHull[0], _convexHull[1]);
                Helper.MoveLine(ls[1], _convexHull[1], _convexHull[2]);
                Helper.MoveLine(ls[2], _convexHull[2], _convexHull[3]);
            }
        }

        public Vector2[] GetVertices()
        {
            return _convexHull.Take(k).ToArray();
        }

        public void Update()
        {
            UpdateStartBounds();
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateQuadraticBezierBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonQuadraticBezier.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonStart.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonQuadraticBezier.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonStart.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonStart.Contains(x, y))
            {
                _hitResult = HitResult.Start;
                return true;
            }
            else if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (ConvexHullAsPolygonContains(x, y)) //_polygonQuadraticBezier.Contains(x, y)
            {
                _hitResult = HitResult.QuadraticBezier;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonStart.Contains(x, y))
            {
                return _quadraticBezier.Start;
            }
            else if (_polygonPoint1.Contains(x, y))
            {
                return _quadraticBezier.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _quadraticBezier.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Start:
                    MoveStart(dx, dy);
                    break;
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.QuadraticBezier:
                    MoveQuadraticBezier(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveQuadraticBezier(dx, dy);
        }

        private void MoveStart(double dx, double dy)
        {
            double x = _quadraticBezier.Start.X - dx;
            double y = _quadraticBezier.Start.Y - dy;
            _quadraticBezier.Start.X = _canvas.EnableSnap ? _canvas.Snap(x, _canvas.SnapX) : x;
            _quadraticBezier.Start.Y = _canvas.EnableSnap ? _canvas.Snap(y, _canvas.SnapY) : y;
            _quadraticBezier.Start = _quadraticBezier.Start;
            Helper.UpdateConnected(_quadraticBezier.Start, dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _quadraticBezier.Point1.X - dx;
            double y1 = _quadraticBezier.Point1.Y - dy;
            _quadraticBezier.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _quadraticBezier.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _quadraticBezier.Point1 = _quadraticBezier.Point1;
            Helper.UpdateConnected(_quadraticBezier.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _quadraticBezier.Point2.X - dx;
            double y2 = _quadraticBezier.Point2.Y - dy;
            _quadraticBezier.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _quadraticBezier.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _quadraticBezier.Point2 = _quadraticBezier.Point2;
            Helper.UpdateConnected(_quadraticBezier.Point2, dx, dy);
        }

        private void MoveQuadraticBezier(double dx, double dy)
        {
            double x = _quadraticBezier.Start.X - dx;
            double y = _quadraticBezier.Start.Y - dy;
            double x1 = _quadraticBezier.Point1.X - dx;
            double y1 = _quadraticBezier.Point1.Y - dy;
            double x2 = _quadraticBezier.Point2.X - dx;
            double y2 = _quadraticBezier.Point2.Y - dy;
            _quadraticBezier.Start.X = _canvas.EnableSnap ? _canvas.Snap(x, _canvas.SnapX) : x;
            _quadraticBezier.Start.Y = _canvas.EnableSnap ? _canvas.Snap(y, _canvas.SnapY) : y;
            _quadraticBezier.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _quadraticBezier.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _quadraticBezier.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _quadraticBezier.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _quadraticBezier.Start = _quadraticBezier.Start;
            _quadraticBezier.Point1 = _quadraticBezier.Point1;
            _quadraticBezier.Point2 = _quadraticBezier.Point2;
            Helper.UpdateConnected(_quadraticBezier.Start, dx, dy);
            Helper.UpdateConnected(_quadraticBezier.Point1, dx, dy);
            Helper.UpdateConnected(_quadraticBezier.Point2, dx, dy);
        }
    }

    public class ArcBounds : IBounds
    {
        private IArc _arc;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonArc;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Point1, Point2, Arc };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public ArcBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IArc arc,
            double size,
            double offset)
        {
            _arc = arc;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonArc = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_arc.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_arc.Point2, ps, ls, _size, _offset);
        }

        private void UpdateArcBounds()
        {
            var ps = _polygonArc.Points;
            var ls = _polygonArc.Lines;
            var p1 = _arc.Point1;
            var p2 = _arc.Point2;

            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);

            Helper.UpdateRectangleBounds(ps, ls, _offset, x, y, width, height);

            UpdateVertices(x, y, width, height);
        }

        private void UpdateVertices(double x, double y, double width, double height)
        {
            _vertices[0] = new Vector2(x, y);
            _vertices[1] = new Vector2(x + width, y);
            _vertices[2] = new Vector2(x + width, y + height);
            _vertices[3] = new Vector2(x, y + height);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateArcBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonArc.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonArc.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonArc.Contains(x, y))
            {
                _hitResult = HitResult.Arc;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                return _arc.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _arc.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Arc:
                    MoveArc(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveArc(dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _arc.Point1.X - dx;
            double y1 = _arc.Point1.Y - dy;
            _arc.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _arc.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _arc.Point1 = _arc.Point1;
            Helper.UpdateConnected(_arc.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _arc.Point2.X - dx;
            double y2 = _arc.Point2.Y - dy;
            _arc.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _arc.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _arc.Point2 = _arc.Point2;
            Helper.UpdateConnected(_arc.Point2, dx, dy);
        }

        private void MoveArc(double dx, double dy)
        {
            double x1 = _arc.Point1.X - dx;
            double y1 = _arc.Point1.Y - dy;
            double x2 = _arc.Point2.X - dx;
            double y2 = _arc.Point2.Y - dy;
            _arc.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _arc.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _arc.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _arc.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _arc.Point1 = _arc.Point1;
            _arc.Point2 = _arc.Point2;
            Helper.UpdateConnected(_arc.Point1, dx, dy);
            Helper.UpdateConnected(_arc.Point2, dx, dy);
        }
    }

    public class RectangleBounds : IBounds
    {
        private IRectangle _rectangle;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonRectangle;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Point1, Point2, Rectangle };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public RectangleBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IRectangle rectangle,
            double size,
            double offset)
        {
            _rectangle = rectangle;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonRectangle = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_rectangle.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_rectangle.Point2, ps, ls, _size, _offset);
        }

        private void UpdateRectangleBounds()
        {
            var ps = _polygonRectangle.Points;
            var ls = _polygonRectangle.Lines;
            var p1 = _rectangle.Point1;
            var p2 = _rectangle.Point2;

            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);

            Helper.UpdateRectangleBounds(ps, ls, _offset, x, y, width, height);

            UpdateVertices(x, y, width, height);
        }

        private void UpdateVertices(double x, double y, double width, double height)
        {
            _vertices[0] = new Vector2(x, y);
            _vertices[1] = new Vector2(x + width, y);
            _vertices[2] = new Vector2(x + width, y + height);
            _vertices[3] = new Vector2(x, y + height);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateRectangleBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonRectangle.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonRectangle.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonRectangle.Contains(x, y))
            {
                _hitResult = HitResult.Rectangle;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                return _rectangle.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _rectangle.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Rectangle:
                    MoveRectangle(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveRectangle(dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _rectangle.Point1.X - dx;
            double y1 = _rectangle.Point1.Y - dy;
            _rectangle.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _rectangle.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _rectangle.Point1 = _rectangle.Point1;
            Helper.UpdateConnected(_rectangle.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _rectangle.Point2.X - dx;
            double y2 = _rectangle.Point2.Y - dy;
            _rectangle.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _rectangle.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _rectangle.Point2 = _rectangle.Point2;
            Helper.UpdateConnected(_rectangle.Point2, dx, dy);
        }

        private void MoveRectangle(double dx, double dy)
        {
            double x1 = _rectangle.Point1.X - dx;
            double y1 = _rectangle.Point1.Y - dy;
            double x2 = _rectangle.Point2.X - dx;
            double y2 = _rectangle.Point2.Y - dy;
            _rectangle.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _rectangle.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _rectangle.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _rectangle.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _rectangle.Point1 = _rectangle.Point1;
            _rectangle.Point2 = _rectangle.Point2;
            Helper.UpdateConnected(_rectangle.Point1, dx, dy);
            Helper.UpdateConnected(_rectangle.Point2, dx, dy);
        }
    }

    public class EllipseBounds : IBounds
    {
        private IEllipse _ellipse;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonEllipse;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Point1, Point2, Ellipse };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public EllipseBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IEllipse ellipse,
            double size,
            double offset)
        {
            _ellipse = ellipse;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonEllipse = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_ellipse.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_ellipse.Point2, ps, ls, _size, _offset);
        }

        private void UpdateEllipseBounds()
        {
            var ps = _polygonEllipse.Points;
            var ls = _polygonEllipse.Lines;
            var p1 = _ellipse.Point1;
            var p2 = _ellipse.Point2;

            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);

            Helper.UpdateRectangleBounds(ps, ls, _offset, x, y, width, height);

            UpdateVertices(x, y, width, height);
        }

        private void UpdateVertices(double x, double y, double width, double height)
        {
            _vertices[0] = new Vector2(x, y);
            _vertices[1] = new Vector2(x + width, y);
            _vertices[2] = new Vector2(x + width, y + height);
            _vertices[3] = new Vector2(x, y + height);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateEllipseBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonEllipse.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonEllipse.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonEllipse.Contains(x, y))
            {
                _hitResult = HitResult.Ellipse;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }

        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                return _ellipse.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _ellipse.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Ellipse:
                    MoveEllipse(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveEllipse(dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _ellipse.Point1.X - dx;
            double y1 = _ellipse.Point1.Y - dy;
            _ellipse.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _ellipse.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _ellipse.Point1 = _ellipse.Point1;
            Helper.UpdateConnected(_ellipse.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _ellipse.Point2.X - dx;
            double y2 = _ellipse.Point2.Y - dy;
            _ellipse.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _ellipse.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _ellipse.Point2 = _ellipse.Point2;
            Helper.UpdateConnected(_ellipse.Point2, dx, dy);
        }

        private void MoveEllipse(double dx, double dy)
        {
            double x1 = _ellipse.Point1.X - dx;
            double y1 = _ellipse.Point1.Y - dy;
            double x2 = _ellipse.Point2.X - dx;
            double y2 = _ellipse.Point2.Y - dy;
            _ellipse.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _ellipse.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _ellipse.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _ellipse.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _ellipse.Point1 = _ellipse.Point1;
            _ellipse.Point2 = _ellipse.Point2;
            Helper.UpdateConnected(_ellipse.Point1, dx, dy);
            Helper.UpdateConnected(_ellipse.Point2, dx, dy);
        }
    }

    public class TextBounds : IBounds
    {
        private IText _text;
        private double _size;
        private double _offset;
        private ICanvas _canvas;
        private IPolygon _polygonText;
        private IPolygon _polygonPoint1;
        private IPolygon _polygonPoint2;
        private bool _isVisible;

        private enum HitResult { None, Point1, Point2, Text };
        private HitResult _hitResult;
        private Vector2[] _vertices;

        public TextBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            ICanvas canvas,
            IText text,
            double size,
            double offset)
        {
            _text = text;
            _size = size;
            _offset = offset;
            _canvas = canvas;

            _hitResult = HitResult.None;

            InitBounds(nativeConverter, canvasFactory);
        }

        private void InitBounds(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory)
        {
            _polygonPoint1 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonPoint2 = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _polygonText = Helper.CreateBoundsPolygon(nativeConverter, canvasFactory, 4);
            _vertices = new Vector2[4];
        }

        private void UpdatePoint1Bounds()
        {
            var ps = _polygonPoint1.Points;
            var ls = _polygonPoint1.Lines;
            Helper.UpdatePointBounds(_text.Point1, ps, ls, _size, _offset);
        }

        private void UpdatePoint2Bounds()
        {
            var ps = _polygonPoint2.Points;
            var ls = _polygonPoint2.Lines;
            Helper.UpdatePointBounds(_text.Point2, ps, ls, _size, _offset);
        }

        private void UpdateTextBounds()
        {
            var ps = _polygonText.Points;
            var ls = _polygonText.Lines;
            var p1 = _text.Point1;
            var p2 = _text.Point2;

            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);

            Helper.UpdateRectangleBounds(ps, ls, _offset, x, y, width, height);

            UpdateVertices(x, y, width, height);
        }

        private void UpdateVertices(double x, double y, double width, double height)
        {
            _vertices[0] = new Vector2(x, y);
            _vertices[1] = new Vector2(x + width, y);
            _vertices[2] = new Vector2(x + width, y + height);
            _vertices[3] = new Vector2(x, y + height);
        }

        public Vector2[] GetVertices()
        {
            return _vertices;
        }

        public void Update()
        {
            UpdatePoint1Bounds();
            UpdatePoint2Bounds();
            UpdateTextBounds();
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public void Show()
        {
            if (!_isVisible)
            {
                foreach (var line in _polygonText.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Add(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Add(line);
                }
                _isVisible = true;
            }
        }

        public void Hide()
        {
            if (_isVisible)
            {
                foreach (var line in _polygonText.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint1.Lines)
                {
                    _canvas.Remove(line);
                }
                foreach (var line in _polygonPoint2.Lines)
                {
                    _canvas.Remove(line);
                }
                _isVisible = false;
            }
        }

        public bool Contains(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                _hitResult = HitResult.Point1;
                return true;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                _hitResult = HitResult.Point2;
                return true;
            }
            else if (_polygonText.Contains(x, y))
            {
                _hitResult = HitResult.Text;
                return true;
            }
            _hitResult = HitResult.None;
            return false;
        }
        
        public IPoint ConnectAt(double x, double y)
        {
            if (_polygonPoint1.Contains(x, y))
            {
                return _text.Point1;
            }
            else if (_polygonPoint2.Contains(x, y))
            {
                return _text.Point2;
            }
            return null;
        }

        public void MoveContaining(double dx, double dy)
        {
            //Debug.WriteLine("_hitResult: {0}", _hitResult);
            switch (_hitResult)
            {
                case HitResult.Point1:
                    MovePoint1(dx, dy);
                    break;
                case HitResult.Point2:
                    MovePoint2(dx, dy);
                    break;
                case HitResult.Text:
                    MoveText(dx, dy);
                    break;
            }
        }

        public void MoveAll(double dx, double dy)
        {
            MoveText(dx, dy);
        }

        private void MovePoint1(double dx, double dy)
        {
            double x1 = _text.Point1.X - dx;
            double y1 = _text.Point1.Y - dy;
            _text.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _text.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _text.Point1 = _text.Point1;
            Helper.UpdateConnected(_text.Point1, dx, dy);
        }

        private void MovePoint2(double dx, double dy)
        {
            double x2 = _text.Point2.X - dx;
            double y2 = _text.Point2.Y - dy;
            _text.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _text.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _text.Point2 = _text.Point2;
            Helper.UpdateConnected(_text.Point2, dx, dy);
        }

        private void MoveText(double dx, double dy)
        {
            double x1 = _text.Point1.X - dx;
            double y1 = _text.Point1.Y - dy;
            double x2 = _text.Point2.X - dx;
            double y2 = _text.Point2.Y - dy;
            _text.Point1.X = _canvas.EnableSnap ? _canvas.Snap(x1, _canvas.SnapX) : x1;
            _text.Point1.Y = _canvas.EnableSnap ? _canvas.Snap(y1, _canvas.SnapY) : y1;
            _text.Point2.X = _canvas.EnableSnap ? _canvas.Snap(x2, _canvas.SnapX) : x2;
            _text.Point2.Y = _canvas.EnableSnap ? _canvas.Snap(y2, _canvas.SnapY) : y2;
            _text.Point1 = _text.Point1;
            _text.Point2 = _text.Point2;
            Helper.UpdateConnected(_text.Point1, dx, dy);
            Helper.UpdateConnected(_text.Point2, dx, dy);
        }
    }

    public class BoundsFactory : IBoundsFactory
    {
        private readonly INativeConverter _nativeConverter;
        private readonly ICanvasFactory _canvasFactory;

        public BoundsFactory(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory)
        {
            _nativeConverter = nativeConverter;
            _canvasFactory = canvasFactory;
        }

        public IBounds Create(ICanvas canvas, IPin pin)
        {
            return new PinBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                pin, 
                15.0,
                0.0);
        }

        public IBounds Create(ICanvas canvas, ILine line)
        {
            return new LineBounds
                (_nativeConverter,
                _canvasFactory, 
                canvas, 
                line, 
                15.0, 
                0.0);
        }

        public IBounds Create(ICanvas canvas, IBezier bezier)
        {
            return new BezierBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                bezier, 
                15.0, 
                0.0);
        }

        public IBounds Create(ICanvas canvas, IQuadraticBezier quadraticBezier)
        {
            return new QuadraticBezierBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                quadraticBezier, 
                15.0, 
                0.0);
        }

        public IBounds Create(ICanvas canvas, IArc arc)
        {
            return new ArcBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                arc, 
                0.0, 
                7.5);
        }

        public IBounds Create(ICanvas canvas, IRectangle rectangle)
        {
            return new RectangleBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                rectangle, 
                0.0, 
                7.5);
        }

        public IBounds Create(ICanvas canvas, IEllipse ellipse)
        {
            return new EllipseBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                ellipse, 
                0.0, 
                7.5);
        }

        public IBounds Create(ICanvas canvas, IText text)
        {
            return new TextBounds(
                _nativeConverter, 
                _canvasFactory, 
                canvas, 
                text, 
                0.0, 
                7.5);
        }
    }
}
