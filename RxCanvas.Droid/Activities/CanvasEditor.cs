// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

namespace RxCanvas.Droid
{
    [Activity(Label = "Canvas Editor")]
    public class CanvasEditor : Activity
    {
        private CanvasView _canvasView;
        private IRepository _repository;
        private Diagram _diagram;

        private void Open()
        {
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
            {
                // get diagram from repository
                _diagram = _repository.Get(diagramId);

                // open string as diagram
                int index = _canvasView.View.Files.IndexOf(_canvasView.View.Files.Where(c => c.Name == "Json").FirstOrDefault());
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_diagram.Model)))
                {
                    var xcanvas = _canvasView.View.Files[index].Read(stream);
                    if (xcanvas != null)
                    {
                        _canvasView.View.ToNative(xcanvas);
                    }
                }
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
        }

        private void Save()
        {
            // save diagram as string
            int index = _canvasView.View.Files.IndexOf(_canvasView.View.Files.Where(c => c.Name == "Json").FirstOrDefault());
            using (var stream = new MemoryStream())
            {
                var xcanvas = _canvasView.View.ToModel();
                if (xcanvas != null)
                {
                    _canvasView.View.Files[index].Write(stream, xcanvas);
                    _diagram.Model = Encoding.UTF8.GetString(stream.ToArray());
                }
            }

            // store diagram in repository
            _diagram.Id = _repository.Save(_diagram);
        }

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);
            Console.WriteLine ("DiagramEditor OnCreate");

            _repository = new SQLiteRepository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            _canvasView = new CanvasView(this);

            RegisterForContextMenu(_canvasView);

            Open();

            // set content view to drawing canvas
            SetContentView(_canvasView);

            //ActivityCompat.InvalidateOptionsMenu (this);
        }

        protected override void OnStop()
        {
            base.OnStop();
            Console.WriteLine ("DiagramEditor OnStop");

            Save();
        }

        protected override void OnPause()
        {
            base.OnPause();
            Console.WriteLine ("DiagramEditor OnPause");

            // stop drawing thread
            _canvasView.Stop();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Console.WriteLine ("DiagramEditor OnResume");

            // start drawing thread
            _canvasView.Start(_canvasView.Holder);
        }

        private bool MenuItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 0:
                    _canvasView.View.Undo();
                    return true;
                case 1:
                    _canvasView.View.Redo();
                    return true;
                case 2:
                    _canvasView.View.Delete();
                    return true;
                case 3:
                    _canvasView.View.Clear();
                    return true;
                case 4:
                    _canvasView.View.Enable(null);
                    _canvasView.Renderer.EnableZoom = true;
                    return true;
                case 5:
                    _canvasView.Renderer.ResetZoom();
                    _canvasView.InvalidateView();
                    return true;
                case 6:
                    _canvasView.View.CreateBlock();
                    return true;
                case 7:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Single Selection").FirstOrDefault());
                    return true;
                case 8:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Multi Selection").FirstOrDefault());
                    return true;
                case 9:
                    _canvasView.View.ToggleSnap();
                    return true;
                case 11:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Pin").FirstOrDefault());
                    return true;
                case 12:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Line").FirstOrDefault());
                    return true;
                case 13:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Bézier").FirstOrDefault());
                    return true;
                case 14:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Quadratic Bézier").FirstOrDefault());
                    return true;
                case 15:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Arc").FirstOrDefault());
                    return true;
                case 16:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Rectangle").FirstOrDefault());
                    return true;
                case 17:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Ellipse").FirstOrDefault());
                    return true;
                case 18:
                    _canvasView.Renderer.EnableZoom = false;
                    _canvasView.View.Enable(_canvasView.View.Editors.Where(e => e.Name == "Text").FirstOrDefault());
                    return true;
                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public static void CreateOptionsMenu(IMenu menu)
        {
            menu.Add(0, 0, 0, "Undo");
            menu.Add(0, 1, 1, "Redo");
            menu.Add(0, 2, 2, "Delete");
            menu.Add(0, 3, 3, "Clear");

            menu.Add(1, 4, 4, "Pan & Zoom");
            menu.Add(1, 5, 5, "Reset Zoom");
            menu.Add(1, 6, 6, "Create Block");
            menu.Add(1, 7, 7, "Single Selection");
            menu.Add(1, 8, 8, "Multi Selection");
            menu.Add(1, 9, 9, "Toggle Snap");

            menu.Add(2, 11, 11, "Pin");
            menu.Add(2, 12, 12, "Line");
            menu.Add(2, 13, 13, "Bézier");
            menu.Add(2, 14, 14, "Quadratic Bézier");
            menu.Add(2, 15, 14, "Arc");
            menu.Add(2, 16, 16, "Rectangle");
            menu.Add(2, 17, 17, "Ellipse");
            menu.Add(2, 18, 18, "Text");
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            CreateOptionsMenu(menu);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            return MenuItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            CreateOptionsMenu(menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return MenuItemSelected(item);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
        }
    }
}
