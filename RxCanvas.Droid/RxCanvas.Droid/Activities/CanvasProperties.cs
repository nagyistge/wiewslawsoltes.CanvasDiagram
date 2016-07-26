// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace RxCanvas.Droid
{
    [Activity(Label = "Canvas Properties")]           
    public class CanvasProperties : Activity
    {
        private Button buttonCancel;
        private Button buttonDelete;
        private Button buttonSave;
        private EditText editTextTitle;
        private EditText editTextModel;
        private IRepository repository;
        private Diagram _diagram;

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.DiagramProperties);

            repository = new SQLiteRepository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            // get diagram from repository
            bool isExistingDiagram = false;
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
            {
                _diagram = repository.Get(diagramId);
                isExistingDiagram = true;
            }
            else
            {
                // create empty diagram
                _diagram = new Diagram()
                {
                    Title = "title",
                    Model = ""
                };
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
                repository.Delete(_diagram);

                Finish();
            };

            // save
            buttonSave.Click += (sender, e) =>
            {
                _diagram.Title = editTextTitle.Text;
                _diagram.Model = editTextModel.Text;
                _diagram.Id = repository.Save(_diagram);

                Finish();
            };

            // set current diagram properties
            editTextTitle.Text = _diagram.Title;
            editTextModel.Text = _diagram.Model;

            Console.WriteLine("CanvasProperties OnCreate");
        }

        protected override void OnResume()
        {
            base.OnResume();

            Console.WriteLine("CanvasProperties OnResume");
        }
    }
}
