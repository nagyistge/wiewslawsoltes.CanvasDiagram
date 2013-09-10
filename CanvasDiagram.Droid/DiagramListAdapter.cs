#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CanvasDiagram.Droid.Core;

#endregion

namespace CanvasDiagram.Droid
{
	#region DiagramListAdapter

	public class DiagramListAdapter : BaseAdapter<Diagram>
	{
		protected Activity context = null;
		protected IList<Diagram> diagrams = new List<Diagram>();

		public DiagramListAdapter (Activity context, IList<Diagram> diagrams) 
			: base()
		{
			this.context = context;
			this.diagrams = diagrams;
		}

		public override Diagram this[int index] 
		{
			get { return diagrams [index]; }
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override int Count
		{
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

	#endregion
}

