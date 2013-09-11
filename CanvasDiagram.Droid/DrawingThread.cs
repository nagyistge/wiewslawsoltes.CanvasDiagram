#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CanvasDiagram.Droid.Core;

#endregion

namespace CanvasDiagram.Droid
{
	#region DrawingThread

	public class DrawingThread : Java.Lang.Thread
	{
		#region Fields

		public ConditionVariable Sync;

		private DrawingService drawingService;
		private ISurfaceHolder surfaceHolder;
		private bool invalidateFlag = false;
		private bool isRunning = false;

		#endregion

		public DrawingThread (ISurfaceHolder surfaceHolder, DrawingService drawingService)
		{
			this.surfaceHolder = surfaceHolder;
			this.drawingService = drawingService;

			Sync = new ConditionVariable (true);
		}

		public void SetRunning(bool run)
		{
			this.isRunning = run;
		}

		public void Invalidate()
		{
			this.invalidateFlag = true;
		}

		public override void Run ()
		{
			Canvas canvas = null;

			while (isRunning) 
			{
				if (this.invalidateFlag == true) 
				{
					canvas = null;

					try 
					{
						canvas = surfaceHolder.LockCanvas (null);
						drawingService.DrawDiagram (canvas);
					} 
					catch(Java.Lang.Exception ex) 
					{ 
						Console.WriteLine ("{0}\n{1}", ex.Message, ex.StackTrace);
					}
					finally 
					{
						if (canvas != null) 
							surfaceHolder.UnlockCanvasAndPost (canvas);
					}

					this.invalidateFlag = false;

					Sync.Close ();
					Sync.Block ();
				}
				else
					continue;
			}
		}
	}

	#endregion
}
