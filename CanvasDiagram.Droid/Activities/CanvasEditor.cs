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
    [Activity(Label = "Canvas Editor")]
    public class CanvasEditor : Activity
    {
        private CanvasView _drawingView;
        private Repository _repository;
        private Diagram _currentDiagram;

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            _repository = new Repository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            // create drawing canvas
            _drawingView = new CanvasView(this);

            RegisterForContextMenu(_drawingView);

            // get diagram from repository
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
                _currentDiagram = _repository.Get(diagramId);
            else
                _currentDiagram = new Diagram();

            // create diagram
            TextSerializer.Deserialize(_currentDiagram.Model, _drawingView.Model.Elements);
            _drawingView.Model.UpdateNextId();
            _drawingView.Model.CurrentModel = TextSerializer.Serialize(_drawingView.Model.Elements);

            // set content view to drawing canvas
            SetContentView(_drawingView);

            //ActivityCompat.InvalidateOptionsMenu (this);

            Console.WriteLine ("DiagramEditor OnCreate");
        }

        protected override void OnStop()
        {
            base.OnStop();

            // store diagram model
            _currentDiagram.Model = TextSerializer.Serialize(_drawingView.Model.Elements);
            _currentDiagram.Id = _repository.Save(_currentDiagram);

            _drawingView.Dispose();

            Console.WriteLine ("DiagramEditor OnStop");
        }

        protected override void OnPause()
        {
            base.OnPause();

            Console.WriteLine ("DiagramEditor OnPause");

            // stop drawing thread
            _drawingView.Model.Stop();
        }

        protected override void OnResume()
        {
            base.OnResume();

            Console.WriteLine ("DiagramEditor OnResume");

            // start drawing thread
            _drawingView.Model.Start(_drawingView.Holder);
        }

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
                        _drawingView.Model.Snapshot();
                        _drawingView.Model.GetCenterPoint(out x, out y);
                        _drawingView.Model.InsertAndGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemInsertOrGate:
                    {
                        float x, y;
                        _drawingView.Model.Snapshot();
                        _drawingView.Model.GetCenterPoint(out x, out y);
                        _drawingView.Model.InsertOrGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemResetDiagram:
                    _drawingView.Model.Snapshot();
                    _drawingView.Model.Reset();
                    return true;
                case ItemResetZoom:
                    _drawingView.Model.ResetZoom();
                    _drawingView.Model.RedrawCanvas();
                    return true;
                case ItemEditUndo:
                    _drawingView.Model.Undo();
                    return true;
                case ItemEditRedo:
                    _drawingView.Model.Redo();
                    return true;
                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public static void CreateOptionsMenu(IMenu menu)
        {
            // insert
            menu.Add(ItemGroupInsert, ItemInsertAndGate, 0, "Insert AndGate");
            menu.Add(ItemGroupInsert, ItemInsertOrGate, 1, "Insert OrGate");
            // edit
            menu.Add(ItemGroupEdit, ItemResetDiagram, 2, "Reset Diagram");
            menu.Add(ItemGroupEdit, ItemResetZoom, 3, "Reset Zoom");
            menu.Add(ItemGroupEdit, ItemEditUndo, 4, "Undo");
            menu.Add(ItemGroupEdit, ItemEditRedo, 5, "Redo");
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            CreateOptionsMenu(menu);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            return HabdleItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            CreateOptionsMenu(menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return HabdleItemSelected(item);
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
