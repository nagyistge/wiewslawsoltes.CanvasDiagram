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
        private CanvasView drawingView;
        private Repository repository;
        private Diagram currentDiagram;

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            repository = new Repository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            // create drawing canvas
            drawingView = new CanvasView(this);

            RegisterForContextMenu(drawingView);

            // get diagram from repository
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
                currentDiagram = repository.Get(diagramId);
            else
                currentDiagram = new Diagram();

            // create diagram
            ModelSerializer.Deserialize(currentDiagram.Model, drawingView.Drawing.Elements);
            drawingView.Drawing.UpdateNextId();
            drawingView.Drawing.CurrentModel = ModelSerializer.Serialize(drawingView.Drawing.Elements);

            // set content view to drawing canvas
            SetContentView(drawingView);

            //ActivityCompat.InvalidateOptionsMenu (this);

            //Console.WriteLine ("DiagramEditor OnCreate");
        }

        protected override void OnStop()
        {
            base.OnStop();

            // store diagram model
            currentDiagram.Model = ModelSerializer.Serialize(drawingView.Drawing.Elements);
            currentDiagram.Id = repository.Save(currentDiagram);

            //Console.WriteLine ("DiagramEditor OnStop");
        }

        protected override void OnPause()
        {
            base.OnPause();

            //Console.WriteLine ("DiagramEditor OnPause");

            // stop drawing thread
            drawingView.Drawing.Stop();
        }

        protected override void OnResume()
        {
            base.OnResume();

            //Console.WriteLine ("DiagramEditor OnResume");

            // start drawing thread
            drawingView.Drawing.Start(drawingView.Holder);
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
                        drawingView.Drawing.Snapshot();
                        drawingView.Drawing.GetCenterPoint(out x, out y);
                        drawingView.Drawing.InsertAndGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemInsertOrGate:
                    {
                        float x, y;
                        drawingView.Drawing.Snapshot();
                        drawingView.Drawing.GetCenterPoint(out x, out y);
                        drawingView.Drawing.InsertOrGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemResetDiagram:
                    drawingView.Drawing.Snapshot();
                    drawingView.Drawing.Reset();
                    return true;
                case ItemResetZoom:
                    drawingView.Drawing.ResetZoom();
                    drawingView.Drawing.RedrawCanvas();
                    return true;
                case ItemEditUndo:
                    drawingView.Drawing.Undo();
                    return true;
                case ItemEditRedo:
                    drawingView.Drawing.Redo();
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
