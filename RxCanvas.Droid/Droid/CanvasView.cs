// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using MathUtil;

namespace RxCanvas.Droid
{
    public class CanvasView : SurfaceView, ISurfaceHolderCallback
    {
        public DrawingView View { get; set; }
        public SurfaceRenderer Renderer { get; set; }

        private SurfaceViewData _viewData;
        private IObservable<MotionEvent> _touch;
        private IDisposable _touches;

        public CanvasView(Context context)
            : base(context)
        {
            Initialize(context);
        }

        public CanvasView(Context context, IAttributeSet attrs)
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

            Renderer = new SurfaceRenderer();
            View = new DrawingView(
                new Assembly[]
                {
                    typeof(Bootstrapper).GetTypeInfo().Assembly,
                    typeof(CanvasView).GetTypeInfo().Assembly
                });

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
                if (_viewData == null)
                {
                    _viewData = new SurfaceViewData();
                }

                ISurfaceHolder holder = surfaceHolder;
                Canvas canvas = null;

                _viewData.Start(() =>
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
                if (_viewData != null)
                {
                    _viewData.Stop();
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Render()
        {
            if (_viewData != null)
            {
                _viewData.Tick(16);
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
                    Renderer.RenderState = SurfaceRenderer.State.Pan;
                }
                else if (count == 1 && action == MotionEventActions.Move)
                {
                    if (Renderer.RenderState == SurfaceRenderer.State.Pan)
                    {
                        Renderer.Pan(e.GetX(), e.GetY());
                        InvalidateView();
                    }
                }
                else if (count == 2 && action == MotionEventActions.PointerDown)
                {
                    Renderer.StartPinchToZoom(e.GetX(0), e.GetY(0), count == 2 ? e.GetX(1) : 0f, count == 2 ? e.GetY(1) : 0f);
                    InvalidateView();
                    Renderer.RenderState = SurfaceRenderer.State.Zoom;
                }
                else if (count == 2 && action == MotionEventActions.Move)
                {
                    if (Renderer.RenderState == SurfaceRenderer.State.Zoom)
                    {
                        Renderer.PinchToZoom(e.GetX(0), e.GetY(0), count == 2 ? e.GetX(1) : 0f, count == 2 ? e.GetY(1) : 0f);
                        InvalidateView();
                    }
                }
                else
                {
                    Renderer.RenderState = SurfaceRenderer.State.None;
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
}
