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
	#region DrawingView

	public class DrawingView : SurfaceView, ISurfaceHolderCallback
	{
		#region Properties

		public DrawingService Service { get; set; }

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
			//SetLayerType (LayerType.Hardware, null);
			
			this.Touch += (sender, e) => HandleTouch (sender, e);
			//this.LongClick += (sender, e) => this.ShowContextMenu();

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
			var index = GetPointerIndex (e);
			float x = e.Event.GetX ();
			float y = e.Event.GetY ();
			float x0 = e.Event.GetX (0);
			float y0 = e.Event.GetY (0);
			float x1 = count == 2 ? e.Event.GetX (1) : 0f;
			float y1 = count == 2 ? e.Event.GetY (1) : 0f;

			if (count == 1 && action == MotionEventActions.Down) 
			{
				Service.HandleOneInputDown (x, y);
				e.Handled = true;
			}
			else if (count == 1 && action == MotionEventActions.Move)
			{
				Service.HandleOneInputMove (x, y);
				e.Handled = true;
			}
			else if (count == 2 && action == MotionEventActions.PointerDown) 
			{
				Service.StartPinchToZoom (x0, y0, x1, y1);
				e.Handled = true;
			}
			else if (count == 2 && action == MotionEventActions.Move) 
			{
				Service.PinchToZoom (x0, y0, x1, y1);
				e.Handled = true;
			}
			else if (action == MotionEventActions.Up)
			{
				Service.FinishCurrentElement (x, y);
				e.Handled = true;
			}
			else if (action == MotionEventActions.PointerUp)
			{
				Service.SetPanStart (index == 0 ? x0 : x1, index == 0 ? y0 : y1);
				e.Handled = true;
			}
			else
			{
				e.Handled = false;
			}

			Service.RedrawCanvas ();
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
