#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.OS.Storage;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CanvasDiagram.Droid.Core;

#endregion

namespace CanvasDiagram.Droid
{
	[Activity (Label = "Diagram Editor")]
	public class DiagramEditor : Activity
	{
		#region Fields

		private DrawingView drawingView;
		private DiagramRepository repository;
		private Diagram currentDiagram;

		#endregion

		#region Activity Lifecycle
		
		protected override void OnCreate (Bundle bundle)
		{
			//RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);

			repository = new DiagramRepository ();

			// create drawing canvas
			drawingView = new DrawingView (this);

			RegisterForContextMenu (drawingView);

			// get diagram from repository
			int diagramId = Intent.GetIntExtra ("DiagramId", 0);
			if (diagramId > 0)
				currentDiagram = repository.Get (diagramId);
			else
				currentDiagram = new Diagram ();

			// create diagram
			Editor.Parse (currentDiagram.Model, drawingView.Service.Elements);
			drawingView.Service.UpdateNextId ();
			drawingView.Service.CurrentModel = Editor.Generate (drawingView.Service.Elements);

			// set content view to drawing canvas
			SetContentView (drawingView);

			//ActivityCompat.InvalidateOptionsMenu (this);

			Console.WriteLine ("DiagramEditor OnCreate");
		}

		protected override void OnStop ()
		{
			base.OnStop ();

			// store diagram model
			currentDiagram.Model = Editor.Generate(drawingView.Service.Elements);
			currentDiagram.Id = repository.Save(currentDiagram);

			Console.WriteLine ("DiagramEditor OnStop");
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			Console.WriteLine ("DiagramEditor OnPause");

			// stop drawing thread
			drawingView.Service.Stop ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			Console.WriteLine ("DiagramEditor OnResume");

			// start drawing thread
			drawingView.Service.Start (drawingView.Holder);
		}
		
		#endregion

		#region Options Menu

		private const int ItemGroupInsert = 0;
		private const int ItemGroupEdit = 1;

		private const int ItemInsertAndGate = 0;
		private const int ItemInsertOrGate = 1;

		private const int ItemResetDiagram = 2;
		private const int ItemResetZoom = 3;
		private const int ItemEditUndo = 4;
		private const int ItemEditRedo = 5;

		public bool HabdleItemSelected(IMenuItem item)
		{
			switch (item.ItemId) 
			{
			case ItemInsertAndGate:
				{
					float x, y;
					drawingView.Service.Snapshot ();
					drawingView.Service.GetCenterPoint (out x, out y);
					drawingView.Service.InsertAndGate (x - 15f, y - 15f, true);
				}
				return true;
			case ItemInsertOrGate:
				{
					float x, y;
					drawingView.Service.Snapshot ();
					drawingView.Service.GetCenterPoint (out x, out y);
					drawingView.Service.InsertOrGate (x - 15f, y - 15f, true);
				}
				return true;
			case ItemResetDiagram:
				drawingView.Service.Snapshot ();
				drawingView.Service.Reset ();
				return true;
			case ItemResetZoom:
				drawingView.Service.ResetZoom ();
				drawingView.Service.RedrawCanvas ();
				return true;
			case ItemEditUndo:
				drawingView.Service.Undo ();
				return true;
			case ItemEditRedo:
				drawingView.Service.Redo ();
				return true;
			default:
				return base.OnContextItemSelected (item);
			}
		}

		public static void CreateOptionsMenu (IMenu menu)
		{
			// insert
			menu.Add (ItemGroupInsert, ItemInsertAndGate, 0, "Insert AndGate");
			menu.Add (ItemGroupInsert, ItemInsertOrGate, 1, "Insert OrGate");
			// edit
			menu.Add (ItemGroupEdit, ItemResetDiagram, 2, "Reset Diagram");
			menu.Add (ItemGroupEdit, ItemResetZoom, 3, "Reset Zoom");
			menu.Add (ItemGroupEdit, ItemEditUndo, 4, "Undo");
			menu.Add (ItemGroupEdit, ItemEditRedo, 5, "Redo");
		}

		public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);

			CreateOptionsMenu (menu);
		}

		public override bool OnContextItemSelected (IMenuItem item)
		{
			return HabdleItemSelected (item);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			CreateOptionsMenu (menu);

			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			return HabdleItemSelected (item);
		}

		#endregion

		#region Instance State

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
		}

		#endregion
	}
}
