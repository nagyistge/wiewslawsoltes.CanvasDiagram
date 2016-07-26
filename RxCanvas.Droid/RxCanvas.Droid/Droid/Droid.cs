// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.OS.Storage;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using RxCanvas.Interfaces;
using RxCanvas.Model;
using RxCanvas.Views;
using MathUtil;

namespace RxCanvas.Droid
{
    public static class VectorUtil
    {
        public static float Dot(PointF a, PointF b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static PointF Multiply(PointF a, float scalar)
        {
            return new PointF(a.X * scalar, a.Y * scalar);
        }

        public static PointF Project(PointF a, PointF b)
        {
            return Multiply(b, Dot(a, b) / Dot(b, b));
        }

        public static PointF Substract(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        public static PointF Add(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        public static PointF NearestPointOnLine(PointF a, PointF b, PointF p)
        {
            // http://en.wikipedia.org/wiki/Vector_projection
            return Add(
                Project(
                    Substract(p, a), 
                    Substract(b, a)), 
                a);
        }
    }

    public static class LineUtil
    {
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static void Middle(ref PointF point, float x1, float y1, float x2, float y2)
        {
            float x = x1 + x2;
            float y = y1 + y2;
            point.Set(x / 2f, y / 2f);
        }
    }

    public class DroidService
    {
        private readonly object _sync = new object();
        private bool _isRunning;
        private Action _action;
        private Thread _thread = null;

        public void Start(Action action)
        {
            if (_thread != null)
            {
                return;
            }

            _action = action;
            _isRunning = true;
            _thread = new Thread(new ThreadStart(Loop));
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            SetRunning(false);
            lock (_sync)
            {
                Monitor.Pulse(_sync);
            }

            _thread.Join();
            _thread = null;
        }

        public void SetAction(Action action)
        {
            lock (_sync)
            {
                _action = action;
            }
        }

        public void SetRunning(bool isRunning)
        {
            _isRunning = isRunning;
        }

        public bool Tick(int timeout)
        {
            if (Monitor.TryEnter(_sync, timeout) == false)
            {
                return false;
            }

            Monitor.Pulse(_sync);
            Monitor.Exit(_sync);
            return true;
        }

        public void Loop()
        {
            while (_isRunning)
            {
                lock (_sync)
                {
                    if (_action != null)
                    {
                        _action();
                    }
                    Monitor.Wait(_sync);
                }
            }
        }
    }

    public class DroidCanvas : ICanvas
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IObservable<Vector2> Downs { get; set; }
        public IObservable<Vector2> Ups { get; set; }
        public IObservable<Vector2> Moves { get; set; }
        public IHistory History { get; set; }
        public IList<INative> Children { get; set; }

        public double Width
        {
            get { return 0.0; }
            set { }
        }

        public double Height
        {
            get { return 0.0; }
            set { }
        }

        public IColor Background { get; set; }
        public bool EnableSnap { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public bool IsCaptured { get; set; }

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public DroidCanvas(ICanvas canvas)
        {
            Background = canvas.Background;
            SnapX = canvas.SnapX;
            SnapY = canvas.SnapY;
            EnableSnap = canvas.EnableSnap;

            History = canvas.History;

            Children = new ObservableCollection<INative>();

            Native = null;
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
            if (Native != null)
            {
                (Native as DroidCanvasPanel).Render();
            }
        }
    }

    public class DroidRenderer
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

        public DroidRenderer()
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

    public class DroidCanvasPanel : SurfaceView, ISurfaceHolderCallback
    {
        public DrawingView View { get; set; }
        public DroidRenderer Renderer { get; set; }

        private DroidService _service;
        private IObservable<MotionEvent> _touch;
        private IDisposable _touches;

        public DroidCanvasPanel(Context context)
            : base(context)
        {
            Initialize(context);
        }

        public DroidCanvasPanel(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize(context);
        }

        public void Initialize(Context context)
        {
            Holder.AddCallback(this);
            SetWillNotDraw(true);

            _touch = Observable.FromEventPattern<TouchEventArgs>(this, "Touch").Select(e => e.EventArgs.Event);
            _touches = _touch.Subscribe((e) => OnTouch(e));

            Renderer = new DroidRenderer();
            View = new DrawingView();

            var canvas = View.Layers.LastOrDefault();

            canvas.Downs = Observable.FromEventPattern<Android.Views.View.TouchEventArgs>(this, "Touch")
                .Where(e => (e.EventArgs.Event.Action & MotionEventActions.Mask) == MotionEventActions.Down && e.EventArgs.Event.PointerCount == 1)
                .Select(e =>
                {
                    float px = e.EventArgs.Event.GetX();
                    float py = e.EventArgs.Event.GetY();

                    double x = canvas.EnableSnap ? 
                            canvas.Snap(px - Renderer.PanX, canvas.SnapX * Renderer.Zoom) : px - Renderer.PanX;

                    double y = canvas.EnableSnap ? 
                            canvas.Snap(py - Renderer.PanY, canvas.SnapY * Renderer.Zoom) : py - Renderer.PanY;

                    x /= Renderer.Zoom;
                    y /= Renderer.Zoom;

                    return new Vector2(x, y);
                });

            canvas.Ups = Observable.FromEventPattern<Android.Views.View.TouchEventArgs>(this, "Touch")
                .Where(e => (e.EventArgs.Event.Action & MotionEventActions.Mask) == MotionEventActions.Up && e.EventArgs.Event.PointerCount == 1)
                .Select(e =>
                {
                    float px = e.EventArgs.Event.GetX();
                    float py = e.EventArgs.Event.GetY();

                    double x = canvas.EnableSnap ? 
                            canvas.Snap(px - Renderer.PanX, canvas.SnapX * Renderer.Zoom) : px - Renderer.PanX;

                    double y = canvas.EnableSnap ? 
                            canvas.Snap(py - Renderer.PanY, canvas.SnapY * Renderer.Zoom) : py - Renderer.PanY;

                    x /= Renderer.Zoom;
                    y /= Renderer.Zoom;

                    return new Vector2(x, y);
                });

            canvas.Moves = Observable.FromEventPattern<Android.Views.View.TouchEventArgs>(this, "Touch")
                .Where(e => (e.EventArgs.Event.Action & MotionEventActions.Mask) == MotionEventActions.Move && e.EventArgs.Event.PointerCount == 1)
                .Select(e =>
                {
                    float px = e.EventArgs.Event.GetX();
                    float py = e.EventArgs.Event.GetY();

                    double x = canvas.EnableSnap ? 
                            canvas.Snap(px - Renderer.PanX, canvas.SnapX * Renderer.Zoom) : px - Renderer.PanX;

                    double y = canvas.EnableSnap ? 
                            canvas.Snap(py - Renderer.PanY, canvas.SnapY * Renderer.Zoom) : py - Renderer.PanY;

                    x /= Renderer.Zoom;
                    y /= Renderer.Zoom;

                    return new Vector2(x, y);
                });

            canvas.Native = this;

            View.Initialize();

            // background layer
            View.Layers[0].Background.A = 0xFF;
            View.Layers[0].Background.R = 0xFF;
            View.Layers[0].Background.G = 0xFF;
            View.Layers[0].Background.B = 0xFF;

            // drawing layer
            View.Layers[1].Background.A = 0xFF;
            View.Layers[1].Background.R = 0xF5;
            View.Layers[1].Background.G = 0xF5;
            View.Layers[1].Background.B = 0xF5;

            View.CreateGrid(600.0, 600.0, 30.0, 0.0, 0.0);
        }

        public void Start(ISurfaceHolder surfaceHolder)
        {
            try
            {
                if (_service == null)
                {
                    _service = new DroidService();
                }

                ISurfaceHolder holder = surfaceHolder;
                Canvas canvas = null;

                _service.Start(() =>
                    {
                        canvas = null;

                        try
                        {
                            canvas = holder.LockCanvas(null);
                            if (canvas != null)
                            {
                                Renderer.Render(canvas, View.Layers);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                        }
                        finally
                        {
                            if (canvas != null)
                            {
                                holder.UnlockCanvasAndPost(canvas);
                            }
                        }
                    });
            }
            catch (Exception ex)
            { 
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Stop()
        {
            try
            {
                if (_service != null)
                {
                    _service.Stop();
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Render()
        {
            if (_service != null)
            {
                _service.Tick(16);
            }
        }

        public void InvalidateView()
        {
            View.Layers.LastOrDefault().Render(null);
        }

        private void OnTouch(MotionEvent e)
        {
            if (Renderer.EnableZoom)
            {
                var action = e.Action & MotionEventActions.Mask;
                int count = e.PointerCount;

                if (count == 1 && action == MotionEventActions.Down)
                {
                    Renderer.StartPan(e.GetX(), e.GetY());
                    Renderer.RenderState = DroidRenderer.State.Pan;
                }
                else if (count == 1 && action == MotionEventActions.Move)
                {
                    if (Renderer.RenderState == DroidRenderer.State.Pan)
                    {
                        Renderer.Pan(e.GetX(), e.GetY());
                        InvalidateView();
                    }
                }
                else if (count == 2 && action == MotionEventActions.PointerDown)
                {
                    Renderer.StartPinchToZoom(e.GetX(0), e.GetY(0), count == 2 ? e.GetX(1) : 0f, count == 2 ? e.GetY(1) : 0f);
                    InvalidateView();
                    Renderer.RenderState = DroidRenderer.State.Zoom;
                }
                else if (count == 2 && action == MotionEventActions.Move)
                {
                    if (Renderer.RenderState == DroidRenderer.State.Zoom)
                    {
                        Renderer.PinchToZoom(e.GetX(0), e.GetY(0), count == 2 ? e.GetX(1) : 0f, count == 2 ? e.GetY(1) : 0f);
                        InvalidateView();
                    }
                }
                else
                {
                    Renderer.RenderState = DroidRenderer.State.None;
                }
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            Console.WriteLine("SurfaceChanged");
            Render();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceCreated");
            Start(this.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceDestroyed");
            Stop();
        }

        protected override void OnDraw(Canvas canvas)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_touches != null)
            {
                _touches.Dispose();
            }
        }
    }

    public class DroidConverter : INativeConverter
    {
        public DroidConverter()
        {
        }

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
