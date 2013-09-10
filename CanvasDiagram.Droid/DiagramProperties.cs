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
	[Activity (Label = "Diagram Properties")]			
	public class DiagramProperties : Activity
	{
		#region Fields

		private Button buttonCancel;
		private Button buttonDelete;
		private Button buttonSave;

		private EditText editTextTitle;
		private EditText editTextModel;

		private DiagramRepository repository;
		private Diagram currentDiagram;

		#endregion

		#region Activity Lifecycle

		protected override void OnCreate (Bundle bundle)
		{
			//RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.DiagramProperties);

			repository = new DiagramRepository ();

			// get diagram from repository
			bool isExistingDiagram = false;
			int diagramId = Intent.GetIntExtra ("DiagramId", 0);
			if (diagramId > 0)
			{
				currentDiagram = repository.Get (diagramId);
				isExistingDiagram = true;
			}
			else
			{
				// create new diagram
				currentDiagram = new Diagram ();
			}

			// connect view elements
			buttonCancel = FindViewById<Button> (Resource.Id.buttonCancel);
			buttonDelete = FindViewById<Button> (Resource.Id.buttonDelete);
			buttonSave = FindViewById<Button> (Resource.Id.buttonSave);
			editTextTitle = FindViewById<EditText> (Resource.Id.editTextTitle);
			editTextModel = FindViewById<EditText> (Resource.Id.editTextModel);

			// enable delete button for existing diagrams
			buttonDelete.Enabled = isExistingDiagram;

			// cancel
			buttonCancel.Click += (sender, e) => 
			{
				Finish ();
			};

			// delete
			buttonDelete.Click += (sender, e) => 
			{
				repository.Delete(currentDiagram);

				Finish ();
			};

			// save
			buttonSave.Click += (sender, e) => 
			{
				currentDiagram.Title = editTextTitle.Text;
				currentDiagram.Model = editTextModel.Text;
				currentDiagram.Id = repository.Save(currentDiagram);

				Finish ();
			};

			// set current diagram properties
			editTextTitle.Text = currentDiagram.Title;
			editTextModel.Text = currentDiagram.Model;

			Console.WriteLine ("DiagramProperties OnCreate");
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			Console.WriteLine ("DiagramProperties OnResume");
		}

		#endregion
	}
}

