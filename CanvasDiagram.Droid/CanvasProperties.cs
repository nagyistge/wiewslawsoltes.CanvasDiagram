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
    [Activity(Label = "Canvas Properties")]           
    public class CanvasProperties : Activity
    {
        private Button buttonCancel;
        private Button buttonDelete;
        private Button buttonSave;
        private EditText editTextTitle;
        private EditText editTextModel;
        private Repository repository;
        private Diagram currentDiagram;

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.DiagramProperties);

            repository = new Repository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            // get diagram from repository
            bool isExistingDiagram = false;
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
            {
                currentDiagram = repository.Get(diagramId);
                isExistingDiagram = true;
            }
            else
            {
                // create new diagram
                currentDiagram = new Diagram();
            }

            // connect view elements
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonDelete = FindViewById<Button>(Resource.Id.buttonDelete);
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            editTextTitle = FindViewById<EditText>(Resource.Id.editTextTitle);
            editTextModel = FindViewById<EditText>(Resource.Id.editTextModel);

            // enable delete button for existing diagrams
            buttonDelete.Enabled = isExistingDiagram;

            // cancel
            buttonCancel.Click += (sender, e) =>
            {
                Finish();
            };

            // delete
            buttonDelete.Click += (sender, e) =>
            {
                repository.Delete(currentDiagram);

                Finish();
            };

            // save
            buttonSave.Click += (sender, e) =>
            {
                currentDiagram.Title = editTextTitle.Text;
                currentDiagram.Model = editTextModel.Text;
                currentDiagram.Id = repository.Save(currentDiagram);

                Finish();
            };

            // set current diagram properties
            editTextTitle.Text = currentDiagram.Title;
            editTextModel.Text = currentDiagram.Model;

            Console.WriteLine("CanvasProperties OnCreate");
        }

        protected override void OnResume()
        {
            base.OnResume();

            Console.WriteLine("CanvasProperties OnResume");
        }
    }
}
