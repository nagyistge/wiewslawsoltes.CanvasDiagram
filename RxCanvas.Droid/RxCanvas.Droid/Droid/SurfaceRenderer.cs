// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Android.Graphics;
using RxCanvas.Interfaces;

namespace RxCanvas.Droid
{
    public class SurfaceRenderer
    {
        public enum State { None, Pan, Zoom};
        public State RenderState { get; set; }

        public bool EnableZoom { get; set; }       
        public float Zoom { get; set; }
        public float PanX { get; set; }
        public float PanY { get; set; }

        private Matrix _matrix;
        private Matrix _savedMatrix;
        private float[] _matrixValues;
        private PointF _start;
        private float _minPinchToZoomDistance;
        private float _previousDist;
        private PointF _middle;

        public SurfaceRenderer()
        {
            RenderState = State.None;

            _matrix = new Matrix();
            _savedMatrix = new Matrix();
            _matrixValues = new float[9];
            _start = new PointF(0f, 0f);

            _minPinchToZoomDistance = 10f;
            _previousDist = 0f;

            Zoom = 1f;
            PanX = 0f;
            PanY = 0f;

            _middle = new PointF(0f, 0f);
        }

        public void ResetZoom()
        {
            Zoom = 1f;
            PanX = 0f;
            PanY = 0f;

            _start.Set(0f, 0f);
            _previousDist = 0f;
            _middle.Set(0f, 0f);
            _matrix.Reset();
            _savedMatrix.Reset();
        }

        public void StartPan(float x, float y)
        {
            _savedMatrix.Set(_matrix);
            _start.Set(x, y);
        }

        public void Pan(float x, float y)
        {
            _matrix.Set(_savedMatrix);
            _matrix.PostTranslate(x - _start.X, y - _start.Y);
            _matrix.GetValues(_matrixValues);

            PanX = _matrixValues[Matrix.MtransX];
            PanY = _matrixValues[Matrix.MtransY];
        }

        public void StartPinchToZoom(float x0, float y0, float x1, float y1)
        {
            _previousDist = LineUtil.Distance(x0, y0, x1, y1);
            if (_previousDist > _minPinchToZoomDistance)
            {
                _savedMatrix.Set(_matrix);
                LineUtil.Middle(ref _middle, x0, y0, x1, y1);
            }
        }

        public void PinchToZoom(float x0, float y0, float x1, float y1)
        {
            float currentDist = LineUtil.Distance(x0, y0, x1, y1);
            if (currentDist > _minPinchToZoomDistance)
            {
                float scale = currentDist / _previousDist;
                _matrix.Set(_savedMatrix);
                _matrix.PostScale(scale, scale, _middle.X, _middle.Y);
                _matrix.GetValues(_matrixValues);

                Zoom = _matrixValues[Matrix.MscaleX];
                PanX = _matrixValues[Matrix.MtransX];
                PanY = _matrixValues[Matrix.MtransY];
            }
        }

        private Color ToNativeColor(IColor color)
        {
            return Color.Argb(color.A, color.R, color.G, color.B);
        }

        public void Render(Canvas canvas, IList<ICanvas> layers)
        {
            canvas.Save();
            canvas.Matrix = _matrix;
            canvas.DrawColor(ToNativeColor(layers.LastOrDefault().Background));

            for (int i = 0; i < layers.Count; i++)
            {
                Render(canvas, layers[i].Children.ToList());
            }

            canvas.Restore();
        }

        private void Render(Canvas canvas, IList<INative> children)
        {
            foreach (var child in children)
            {
                if (child is IPin)
                {
                    Render(canvas, child as IPin);
                }
                else if (child is ILine)
                {
                    Render(canvas, child as ILine);
                }
                else if (child is IBezier)
                {
                    Render(canvas, child as IBezier);
                }
                else if (child is IQuadraticBezier)
                {
                    Render(canvas, child as IQuadraticBezier);
                }
                else if (child is IArc)
                {
                    Render(canvas, child as IArc);
                }
                else if (child is IRectangle)
                {
                    Render(canvas, child as IRectangle);
                }
                else if (child is IEllipse)
                {
                    Render(canvas, child as IEllipse);
                }
                else if (child is IText)
                {
                    Render(canvas, child as IText);
                }
                else if (child is IBlock)
                {
                    Render(canvas, (child as IBlock).Children.ToList());
                }
            }
        }

        private void Render(Canvas canvas, IPin pin)
        {
            var paint = new Paint()
            {
                Color = Color.Argb(0xFF, 0x00, 0x00, 0x00),
                StrokeWidth = (float)0.0,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Fill);

            double x = pin.Point.X - 4.0;
            double y = pin.Point.Y - 4.0;
            double width = 8.0;
            double height = 8.0;

            canvas.DrawOval(
                new RectF(
                    (float)x, 
                    (float)y, 
                    (float)(x + width), 
                    (float)(y + height)), 
                paint);

            paint.Dispose();
        }

        private void Render(Canvas canvas, ILine line)
        {
            var paint = new Paint() 
            {
                Color = ToNativeColor(line.Stroke),
                StrokeWidth = (float)line.StrokeThickness,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(
                (float)line.Point1.X, 
                (float)line.Point1.Y, 
                (float)line.Point2.X, 
                (float)line.Point2.Y, 
                paint);

            paint.Dispose();
        }

        private void Render(Canvas canvas, IBezier bezier)
        {
            var paint = new Paint()
            {
                Color = ToNativeColor(bezier.Stroke),
                StrokeWidth = (float)bezier.StrokeThickness,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Stroke);

            var path = new Path();
            path.SetLastPoint(
                (float)bezier.Start.X, 
                (float)bezier.Start.Y);
            path.CubicTo(
                (float)bezier.Point1.X, 
                (float)bezier.Point1.Y, 
                (float)bezier.Point2.X, 
                (float)bezier.Point2.Y, 
                (float)bezier.Point3.X, 
                (float)bezier.Point3.Y);

            canvas.DrawPath(path, paint);

            path.Dispose();
            paint.Dispose();
        }

        private void Render(Canvas canvas, IQuadraticBezier quadraticBezier)
        {
            var paint = new Paint()
            {
                Color = ToNativeColor(quadraticBezier.Stroke),
                StrokeWidth = (float)quadraticBezier.StrokeThickness,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Stroke);

            var path = new Path();
            path.SetLastPoint(
                (float)quadraticBezier.Start.X, 
                (float)quadraticBezier.Start.Y);
            path.QuadTo(
                (float)quadraticBezier.Point1.X, 
                (float)quadraticBezier.Point1.Y, 
                (float)quadraticBezier.Point2.X, 
                (float)quadraticBezier.Point2.Y);

            canvas.DrawPath(path, paint);

            path.Dispose();
            paint.Dispose();
        }

        private void Render(Canvas canvas, IArc arc)
        {
            double x = Math.Min(arc.Point1.X, arc.Point2.X);
            double y = Math.Min(arc.Point1.Y, arc.Point2.Y);
            double width = Math.Abs(arc.Point2.X - arc.Point1.X);
            double height = Math.Abs(arc.Point2.Y - arc.Point1.Y);

            if (width > 0.0 && height > 0.0)
            {
                var paint = new Paint() 
                {
                    Color = ToNativeColor(arc.Stroke),
                    StrokeWidth = (float)arc.StrokeThickness,
                    AntiAlias = true,
                    StrokeCap = Paint.Cap.Butt
                };
                paint.SetStyle(Paint.Style.Stroke);

                canvas.DrawArc(
                    new RectF(
                        (float)x, 
                        (float)y, 
                        (float)(x + width), 
                        (float)(y + height)), 
                    (float)arc.StartAngle, 
                    (float)arc.SweepAngle, 
                    false, 
                    paint);

                paint.Dispose();
            }
        }

        private void Render(Canvas canvas, IRectangle rectangle)
        {
            var paint = new Paint() 
            {
                Color = ToNativeColor(rectangle.Stroke),
                StrokeWidth = (float)rectangle.StrokeThickness,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Stroke);

            double x = Math.Min(rectangle.Point1.X, rectangle.Point2.X);
            double y = Math.Min(rectangle.Point1.Y, rectangle.Point2.Y);
            double width = Math.Abs(rectangle.Point2.X - rectangle.Point1.X);
            double height = Math.Abs(rectangle.Point2.Y - rectangle.Point1.Y);

            canvas.DrawRect(
                (float)x, 
                (float)y, 
                (float)(x + width), 
                (float)(y + height), 
                paint);

            paint.Dispose();
        }

        private void Render(Canvas canvas, IEllipse ellipse)
        {
            var paint = new Paint() 
            {
                Color = ToNativeColor(ellipse.Stroke),
                StrokeWidth = (float)ellipse.StrokeThickness,
                AntiAlias = true,
                StrokeCap = Paint.Cap.Butt
            };
            paint.SetStyle(Paint.Style.Stroke);

            double x = Math.Min(ellipse.Point1.X, ellipse.Point2.X);
            double y = Math.Min(ellipse.Point1.Y, ellipse.Point2.Y);
            double width = Math.Abs(ellipse.Point2.X - ellipse.Point1.X);
            double height = Math.Abs(ellipse.Point2.Y - ellipse.Point1.Y);

            canvas.DrawOval(
                new RectF(
                    (float)x, 
                    (float)y, 
                    (float)(x + width), 
                    (float)(y + height)), 
                paint);

            paint.Dispose();
        }

        private void Render(Canvas canvas, IText text)
        {
            var paint = new Paint() 
            {
                Color = ToNativeColor(text.Foreground),
                AntiAlias = true,
                StrokeWidth = 1f,
                TextAlign = 
                        text.HorizontalAlignment == 0 ? Paint.Align.Left :
                        text.HorizontalAlignment == 1 ? Paint.Align.Center : 
                        text.HorizontalAlignment == 2 ? Paint.Align.Right : Paint.Align.Center,
                TextSize = (float)text.Size,
                SubpixelText = true,
            };

            double x = Math.Min(text.Point1.X, text.Point2.X);
            double y = Math.Min(text.Point1.Y, text.Point2.Y);
            double width = Math.Abs(text.Point2.X - text.Point1.X);
            double height = Math.Abs(text.Point2.Y - text.Point1.Y);
            float verticalSize = paint.Descent() + paint.Ascent();
            float verticalOffset = 
                text.VerticalAlignment == 0 ? 0.0f : 
                text.VerticalAlignment == 1 ? verticalSize / 2.0f : 
                text.VerticalAlignment == 2 ? verticalSize : verticalSize / 2.0f;

            canvas.DrawText(
                text.Text, 
                (float)(x + width / 2.0), 
                (float)(y + (height / 2.0) - verticalOffset), 
                paint);
        }
    }
}
