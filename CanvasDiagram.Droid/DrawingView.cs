#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

#endregion

namespace CanvasDiagram.Droid
{
	#region InputActions

	public static class InputActions
	{
		public const int None = 0;
		public const int Hitest = 2;
		public const int Move = 4;
		public const int StartZoom = 8;
		public const int Zoom = 16;
		public const int Merge = 32;
		public const int StartPan = 64;
	}

	#endregion

	#region InputArgs

	public class InputArgs
	{
		public int Action;
		public float X;
		public float Y;
		public int Index;
		public float X0;
		public float Y0;
		public float X1;
		public float Y1;
	}

	#endregion

	#region DrawingView

	public class DrawingView : SurfaceView, ISurfaceHolderCallback
	{
		#region Properties

		public DrawingService Service { get; set; }

		#endregion

		#region Fields

		public InputArgs Args = new InputArgs ();

		#endregion
		
		#region Constructor

		public DrawingView (Context context)
			: base(context)
		{
			Initialize (context);
		}

		public DrawingView (Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize (context);
		}

		#endregion

		#region Initialize

		private void Initialize (Context context)
		{
			Holder.AddCallback (this);
			SetWillNotDraw (true);

			this.Touch += (sender, e) => HandleTouch (sender, e);

			Service = new DrawingService ();
			Service.Initialize ();
		}

		#endregion

		#region ISurfaceHolderCallback

		public void SurfaceChanged (ISurfaceHolder holder, Format format, int width, int height)
		{
			Console.WriteLine ("SurfaceChanged");

			Service.SurfaceWidth = width;
			Service.SurfaceHeight = height;

			Service.RedrawCanvas ();
		}

		public void SurfaceCreated (ISurfaceHolder holder)
		{
			Console.WriteLine ("SurfaceCreated");

			Service.Start (this.Holder);
		}

		public void SurfaceDestroyed (ISurfaceHolder holder)
		{
			Console.WriteLine ("SurfaceDestroyed");

			Service.Stop ();
		}

		#endregion

		#region Input Touch

		private static int GetPointerIndex (TouchEventArgs e)
		{
			int index = ((int)(e.Event.Action & MotionEventActions.PointerIndexMask) 
			             >> (int)MotionEventActions.PointerIndexShift) == 0 ? 1 : 0;
			return index;
		}

		private void HandleTouch(object sender, TouchEventArgs e)
		{
			var action = e.Event.Action & MotionEventActions.Mask;
			int count = e.Event.PointerCount;

			Args.X = e.Event.GetX ();
			Args.Y = e.Event.GetY ();
			Args.Index = GetPointerIndex (e);
			Args.X0 = e.Event.GetX (0);
			Args.Y0 = e.Event.GetY (0);
			Args.X1 = count == 2 ? e.Event.GetX (1) : 0f;
			Args.Y1 = count == 2 ? e.Event.GetY (1) : 0f;

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

			Service.RedrawCanvas (Args);
		}

		#endregion

		#region OnDraw

		protected override void OnDraw (Canvas canvas)
		{
		}

		#endregion
	}

	#endregion
}
