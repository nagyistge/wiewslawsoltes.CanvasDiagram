using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace CanvasDiagram.Droid
{
    public class CanvasView : SurfaceView, ISurfaceHolderCallback
    {
        public CanvasDrawing Drawing { get; set; }
        public InputArgs Args = new InputArgs();

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
            Holder.AddCallback(this);
            SetWillNotDraw(true);

            this.Touch += (sender, e) => HandleTouch(sender, e);

            Drawing = new CanvasDrawing();
            Drawing.Initialize();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            Console.WriteLine("SurfaceChanged");

            Drawing.SurfaceWidth = width;
            Drawing.SurfaceHeight = height;
            Drawing.RedrawCanvas();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceCreated");

            Drawing.Start(this.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceDestroyed");

            Drawing.Stop();
        }

        private static int GetPointerIndex(TouchEventArgs e)
        {
            int index = ((int)(e.Event.Action & MotionEventActions.PointerIndexMask)
                        >> (int)MotionEventActions.PointerIndexShift) == 0 ? 1 : 0;
            return index;
        }

        private void HandleTouch(object sender, TouchEventArgs e)
        {
            var action = e.Event.Action & MotionEventActions.Mask;
            int count = e.Event.PointerCount;

            Args.X = e.Event.GetX();
            Args.Y = e.Event.GetY();
            Args.Index = GetPointerIndex(e);
            Args.X0 = e.Event.GetX(0);
            Args.Y0 = e.Event.GetY(0);
            Args.X1 = count == 2 ? e.Event.GetX(1) : 0f;
            Args.Y1 = count == 2 ? e.Event.GetY(1) : 0f;

            if (count == 1 && action == MotionEventActions.Down)
                Args.Action = InputActions.Hitest;
            else if (count == 1 && action == MotionEventActions.Move)
                Args.Action = InputActions.Move;
            else if (count == 2 && action == MotionEventActions.PointerDown)
                Args.Action = InputActions.StartZoom;
            else if (count == 2 && action == MotionEventActions.Move)
                Args.Action = InputActions.Zoom;
            else if (action == MotionEventActions.Up)
                Args.Action = InputActions.Merge;
            else if (action == MotionEventActions.PointerUp)
                Args.Action = InputActions.StartPan;
            else
                Args.Action = InputActions.None;

            Drawing.RedrawCanvas(Args);
        }

        protected override void OnDraw(Canvas canvas)
        {
        }
    }
}
