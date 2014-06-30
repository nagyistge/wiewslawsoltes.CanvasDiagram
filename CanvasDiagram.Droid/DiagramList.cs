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
    [Activity(Label = "Canvas Diagram", MainLauncher = true)]          
    public class DiagramList : Activity
    {
        private ListView listViewDiagrams;
        private Repository repository;
        private IList<Diagram> diagrams;

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.DiagramList);

            repository = new Repository(
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                    "diagrams.db"));

            // connect view elements
            listViewDiagrams = FindViewById<ListView>(Resource.Id.listViewDiagrams);

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

            Console.WriteLine("DiagramList OnResume");
        }

        protected override void OnResume()
        {
            base.OnResume();

            // get diagrams from repository
            diagrams = repository.GetAll();

            // set diagram list adapter
            var adapter = new DiagramListAdapter(this, diagrams);
            listViewDiagrams.Adapter = adapter;

            Console.WriteLine("DiagramList OnResume");
        }

        public void DeleteAll()
        {
            repository.RemoveAll();
            diagrams.Clear();
            (listViewDiagrams.Adapter as DiagramListAdapter).NotifyDataSetChanged();
        }

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
                            .SetNegativeButton("No", (sender, e) =>
                            {
                            })
                            .SetPositiveButton("Yes", (sender, e) => DeleteAll())
                            .Show();
                    }
                    return true;
                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public static void CreateOptionsMenu(IMenu menu)
        {
            menu.Add(ItemGroupAdd, ItemAddDiagram, 0, "Add Diagram");
            menu.Add(ItemGroupAdd, ItemDeleteAll, 1, "Delete All");
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
    }
}
