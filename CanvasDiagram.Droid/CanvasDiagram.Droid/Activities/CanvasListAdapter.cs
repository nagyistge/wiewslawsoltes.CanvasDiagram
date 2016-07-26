using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace CanvasDiagram.Droid
{
    public class CanvasListAdapter : BaseAdapter<Diagram>
    {
        private readonly Activity context = null;
        private readonly IList<Diagram> diagrams = new List<Diagram> ();

        public CanvasListAdapter (Activity context, IList<Diagram> diagrams)
            : base ()
        {
            this.context = context;
            this.diagrams = diagrams;
        }

        public override Diagram this [int position] {
            get { return diagrams [position]; }
        }

        public override long GetItemId (int position)
        {
            return position;
        }

        public override int Count {
            get { return diagrams.Count; }
        }

        public override View GetView (int position, View convertView, ViewGroup parent)
        {
            var diagram = diagrams [position];
            var view = (convertView ??
                       context.LayoutInflater.Inflate (
                           Android.Resource.Layout.SimpleListItem1,
                           parent,
                           false)) as TextView;

            view.SetText (diagram.Title, TextView.BufferType.Normal);

            return view;
        }
    }
}
