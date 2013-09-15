#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CanvasDiagram.Droid.Core;

#endregion

namespace CanvasDiagram.Droid
{
	#region DiagramList

	[Activity (Label = "Canvas Diagram", MainLauncher = true)]			
	public class DiagramList : Activity
	{
		#region Fields

		private ListView listViewDiagrams;

		private DiagramRepository repository;
		private List<Diagram> diagrams;

		#endregion

		#region Activity Lifecycle

		protected override void OnCreate (Bundle bundle)
		{
			//RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.DiagramList);

			/*

			//
			// Begin: new Core.Test
			//

			try
			{
				var sb = new StringBuilder ();
				var nl = CanvasDiagram.Core.Test.Constants.NewLine;

				sb.Append ("PS;0;255;255;0;0;1;1.0;4.0"); sb.Append (nl);
				sb.Append ("LS;1;255;255;0;0;0;1.0"); sb.Append (nl);
				sb.Append ("RS;2;255;255;0;0;0;1.0"); sb.Append (nl);
				sb.Append ("CS;3;255;255;0;0;0;1.0"); sb.Append (nl);
				sb.Append ("AS;4;255;255;0;0;0;1.0"); sb.Append (nl);
				sb.Append ("TS;5;255;255;0;0;0;1.0;1;1;14.0"); sb.Append (nl);

				var data = sb.ToString ();
				var factory = new CanvasDiagram.Droid.Renderers.RendererFactory ("Renderer", "CanvasDiagram.Droid.Renderers");
				var model = CanvasDiagram.Core.Test.Editor.Parse (data, factory);
			}
			catch (Exception ex)
			{
				Console.WriteLine ("Parse Error: {0}", ex.Message);
				Console.WriteLine (ex.StackTrace);
			}

			//
			// End: new Core.Test
			//

			*/

			repository = new DiagramRepository ();

			// connect view elements
			listViewDiagrams = FindViewById<ListView> (Resource.Id.listViewDiagrams);

			// edit diagram model
			listViewDiagrams.ItemClick += (sender, e) => 
			{
				var diagramEidtor = new Intent(this, typeof(DiagramEditor));
				diagramEidtor.PutExtra("DiagramId", diagrams[e.Position].Id);

				StartActivity(diagramEidtor);
			};

			// edit diagram properties
			listViewDiagrams.ItemLongClick += (sender, e) => 
			{
				var diagramProperties = new Intent(this, typeof(DiagramProperties));
				diagramProperties.PutExtra("DiagramId", diagrams[e.Position].Id);

				StartActivity(diagramProperties);
			};

			Console.WriteLine ("DiagramList OnResume");
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			// get diagrams from repository
			diagrams = repository.GetAll ();

			// set diagram list adapter
			var adapter = new DiagramListAdapter (this, diagrams);
			listViewDiagrams.Adapter = adapter;

			Console.WriteLine ("DiagramList OnResume");
		}

		#endregion

		#region Repository

		public void DeleteAll()
		{
			repository.RemoveAll ();
			diagrams.Clear ();
			(listViewDiagrams.Adapter as DiagramListAdapter).NotifyDataSetChanged ();
		}

		#endregion

		#region Options Menu

		private const int ItemGroupAdd = 0;

		private const int ItemAddDiagram = 0;
		private const int ItemDeleteAll = 1;

		public bool HabdleItemSelected(IMenuItem item)
		{
			switch (item.ItemId) 
			{
				case ItemAddDiagram:
					StartActivity(typeof(DiagramProperties));
					return true;
				case ItemDeleteAll:
					{
						AlertDialog.Builder ab = new AlertDialog.Builder(this);
						ab.SetMessage("Delete All diagrams?")
					      .SetNegativeButton("No", (sender, e) => { } )
					      .SetPositiveButton("Yes", (sender, e) => DeleteAll())
						  .Show();
					}
					return true;
				default:
					return base.OnContextItemSelected (item);
			}
		}

		public static void CreateOptionsMenu (IMenu menu)
		{
			menu.Add (ItemGroupAdd, ItemAddDiagram, 0, "Add Diagram");
			menu.Add (ItemGroupAdd, ItemDeleteAll, 1, "Delete All");
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
	}

	#endregion
}
