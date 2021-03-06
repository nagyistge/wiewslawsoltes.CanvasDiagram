// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace RxCanvas.Droid
{
    public class CanvasListAdapter : BaseAdapter<Diagram>
    {
        private readonly Activity context = null;
        private readonly IList<Diagram> diagrams = new List<Diagram>();

        public CanvasListAdapter(Activity context, IList<Diagram> diagrams)
            : base()
        {
            this.context = context;
            this.diagrams = diagrams;
        }

        public override Diagram this [int index]
        {
            get { return diagrams[index]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return diagrams.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var diagram = diagrams[position];
            var view = (convertView ??
                       context.LayoutInflater.Inflate(
                           Android.Resource.Layout.SimpleListItem1,
                           parent, 
                           false)) as TextView;

            view.SetText(diagram.Title, TextView.BufferType.Normal);

            return view;
        }
    }
}
