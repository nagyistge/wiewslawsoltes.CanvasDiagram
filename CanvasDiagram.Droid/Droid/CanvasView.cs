using System;
using System.Linq;
using System.Reactive.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace CanvasDiagram.Droid
{
    public class CanvasView : SurfaceView, ISurfaceHolderCallback
    {
        private IObservable<MotionEvent> _touch;
        private IDisposable _touches;

        public CanvasViewModel Model { get; private set; }
        public InputArgs Args { get; private set; }

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

        private void Initialize(Context context)
        {
            Args = new InputArgs();

            Holder.AddCallback(this);
            SetWillNotDraw(true);

            //this.Touch += (sender, e) => OnTouch(e.Event);
            _touch = Observable.FromEventPattern<TouchEventArgs>(this, "Touch").Select(e => e.EventArgs.Event);
            _touches = _touch.Subscribe((e) => OnTouch(e));

            Model = new CanvasViewModel();
            Model.Initialize();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            Console.WriteLine("SurfaceChanged");

            Model.SurfaceWidth = width;
            Model.SurfaceHeight = height;
            Model.RedrawCanvas();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceCreated");

            Model.Start(this.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceDestroyed");

            Model.Stop();
        }

        private int ToPointerIndex(MotionEvent e)
        {
            return ((int)(e.Action & MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift) == 0 ? 1 : 0;
        }

        private void OnTouch(MotionEvent e)
        {
            var action = e.Action & MotionEventActions.Mask;
            int count = e.PointerCount;
            Args.X = e.GetX();
            Args.Y = e.GetY();
            Args.Index = ToPointerIndex(e);
            Args.X0 = e.GetX(0);
            Args.Y0 = e.GetY(0);
            Args.X1 = count == 2 ? e.GetX(1) : 0f;
            Args.Y1 = count == 2 ? e.GetY(1) : 0f;
            if (count == 1 && action == MotionEventActions.Down)
            {
                Args.Action = InputAction.Hitest;
            }
            else if (count == 1 && action == MotionEventActions.Move)
            {
                Args.Action = InputAction.Move;
            }
            else if (count == 2 && action == MotionEventActions.PointerDown)
            {
                Args.Action = InputAction.StartZoom;
            }
            else if (count == 2 && action == MotionEventActions.Move)
            {
                Args.Action = InputAction.Zoom;
            }
            else if (action == MotionEventActions.Up)
            {
                Args.Action = InputAction.Merge;
            }
            else if (action == MotionEventActions.PointerUp)
            {
                Args.Action = InputAction.StartPan;
            }
            else
            {
                Args.Action = InputAction.None;
            }
            Model.RedrawCanvas(Args);
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
