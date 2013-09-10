
#region References

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

#endregion

namespace CanvasDiagram.Core.Test
{
	#region Context

	public class Context
	{
		#region Model

		public Model Model { get; private set; }

		#endregion

		#region Argument Lists

		public IList<string[]> PinArgs { get; private set; }
		public IList<string[]> LineArgs { get; private set; }
		public IList<string[]> RectangleArgs { get; private set; }
		public IList<string[]> CircleArgs { get; private set; }
		public IList<string[]> ArcArgs { get; private set; }
		public IList<string[]> TextArgs { get; private set; }
		public IList<string[]> CustomArgs { get; private set; }
		public IList<string[]> ReferenceArgs { get; private set; }

		#endregion

		#region Renderer Factories

		public Func<IRenderer<Pin, PinStyle>> PinRenderer { get; private set; }
		public Func<IRenderer<Line, LineStyle>> LineRenderer { get; private set; }
		public Func<IRenderer<Rectangle, RectangleStyle>> RectangleRenderer { get; private set; }
		public Func<IRenderer<Arc, ArcStyle>> ArcRenderer { get; private set; }
		public Func<IRenderer<Circle, CircleStyle>> CircleRenderer { get; private set; }
		public Func<IRenderer<Text, TextStyle>> TextRenderer { get; private set; }

		#endregion

		#region Constructor

		public Context (IRendererFactory factory)
		{
			Model = new Model ();

			PinArgs = new List<string[]> ();
			LineArgs = new List <string[]> ();
			RectangleArgs = new List <string[]> ();
			CircleArgs = new List <string[]> ();
			ArcArgs = new List <string[]> ();
			TextArgs = new List <string[]> ();
			CustomArgs = new List <string[]> ();
			ReferenceArgs = new List <string[]> ();

			PinRenderer = factory.GetActivator<Pin, PinStyle> ();
			LineRenderer = factory.GetActivator<Line, LineStyle> ();
			RectangleRenderer = factory.GetActivator<Rectangle, RectangleStyle> ();
			ArcRenderer = factory.GetActivator<Arc, ArcStyle> ();
			CircleRenderer = factory.GetActivator<Circle, CircleStyle> ();
			TextRenderer = factory.GetActivator<Text, TextStyle> ();
		}

		#endregion
	}

	#endregion
}
