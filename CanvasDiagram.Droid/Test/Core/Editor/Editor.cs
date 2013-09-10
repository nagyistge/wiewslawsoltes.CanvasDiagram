
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
	#region Util

	public static class Util
	{
		#region Util

		public static bool Compare(string a, string b)
		{
			return string.Compare (a, b, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public static string[] GetLines(string model)
		{
			return model.Split (Constants.LineSeparators, StringSplitOptions.RemoveEmptyEntries);
		}

		public static string[] GetArgs(string line)
		{
			return line.Split (Constants.ArgSeparators, StringSplitOptions.RemoveEmptyEntries);
		}

		public static bool GetBool(string flag)
		{
			return (int.Parse (flag) == 1) ? true : false;
		}

		public static int GetInt(string number)
		{
			return int.Parse (number);
		}

		public static float GetFloat(string number)
		{
			return float.Parse (number, System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
		}

		public static string GetString(float number)
		{
			return number.ToString (System.Globalization.CultureInfo.GetCultureInfo ("en-GB"));
		}

		#endregion
	}

	#endregion

	#region Editor Find Extensions

	public static class EditorFindExtensions
	{
		#region Find Style

		public static PinStyle FindPinStyle (this Context ctx, int styleId)
		{
			PinStyle style;
			if (!ctx.Model.PinStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find PinStyle.");
			return style;
		}

		public static LineStyle FindLineStyle (this Context ctx, int styleId)
		{
			LineStyle style;
			if (!ctx.Model.LineStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find LineStyle.");
			return style;
		}

		public static RectangleStyle FindRectangleStyle (this Context ctx, int styleId)
		{
			RectangleStyle style;
			if (!ctx.Model.RectangleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find RectangleStyle.");
			return style;
		}

		public static CircleStyle FindCircleStyle (this Context ctx, int styleId)
		{
			CircleStyle style;
			if (!ctx.Model.CircleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find CircleStyle.");
			return style;
		}

		public static ArcStyle FindArcStyle (this Context ctx, int styleId)
		{
			ArcStyle style;
			if (!ctx.Model.ArcStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find ArcStyles.");
			return style;
		}

		public static TextStyle FindTextStyle (this Context ctx, int styleId)
		{
			TextStyle style;
			if (!ctx.Model.TextStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find TextStyles.");
			return style;
		}

		#endregion

		#region Find Primitive

		public static Pin FindPin (this Context ctx, int id)
		{
			Pin pin;
			if (!ctx.Model.Pins.TryGetValue (id, out pin))
				throw new ArgumentException ("Failed to find Pin");
			return pin;
		}

		public static Line FindLine (this Context ctx, int id)
		{
			Line line;
			if (!ctx.Model.Lines.TryGetValue (id, out line))
				throw new ArgumentException ("Failed to find Line");
			return line;
		}

		public static Rectangle FindRectangle (this Context ctx, int id)
		{
			Rectangle rectangle;
			if (!ctx.Model.Rectangles.TryGetValue (id, out rectangle))
				throw new ArgumentException ("Failed to find Rectangle");
			return rectangle;
		}

		public static Circle FindCircle (this Context ctx, int id)
		{
			Circle circle;
			if (!ctx.Model.Circles.TryGetValue (id, out circle))
				throw new ArgumentException ("Failed to find Circle");
			return circle;
		}

		public static Arc FindArc (this Context ctx, int id)
		{
			Arc arc;
			if (!ctx.Model.Arcs.TryGetValue (id, out arc))
				throw new ArgumentException ("Failed to find Arc");
			return arc;
		}

		public static Text FindText (this Context ctx, int id)
		{
			Text text;
			if (!ctx.Model.Texts.TryGetValue (id, out text))
				throw new ArgumentException ("Failed to find Text");
			return text;
		}

		#endregion
	}

	#endregion

	#region Editor Add Extensions

	public static class EditorAddExtensions
	{
		#region Add Style

		public static void Add (this Context ctx, PinStyle style)
		{
			if (!ctx.Model.PinStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to PinStyles dictionary.");
		}

		public static void Add (this Context ctx, LineStyle style)
		{
			if (!ctx.Model.LineStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to LineStyles dictionary.");
		}

		public static void Add (this Context ctx, RectangleStyle style)
		{
			if (!ctx.Model.RectangleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to RectangleStyles dictionary.");
		}

		public static void Add (this Context ctx, CircleStyle style)
		{
			if (!ctx.Model.CircleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to CircleStyles dictionary.");
		}

		public static void Add (this Context ctx, ArcStyle style)
		{
			if (!ctx.Model.ArcStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to ArcStyles dictionary.");
		}

		public static void Add (this Context ctx, TextStyle style)
		{
			if (!ctx.Model.TextStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to TextStyles dictionary.");
		}

		#endregion

		#region Add Primitive

		public static void Add (this Context ctx, Pin pin)
		{
			if (!ctx.Model.Pins.TryAdd (pin.Id, pin))
				throw new ArgumentException ("Failed Add to Pins dictionary.");
		}

		public static void Add (this Context ctx, Line line)
		{
			if (!ctx.Model.Lines.TryAdd (line.Id, line))
				throw new ArgumentException ("Failed Add to Lines dictionary.");
		}

		public static void Add (this Context ctx, Rectangle rectangle)
		{
			if (!ctx.Model.Rectangles.TryAdd (rectangle.Id, rectangle))
				throw new ArgumentException ("Failed Add to Rectangles dictionary.");
		}

		public static void Add (this Context ctx, Circle circle)
		{
			if (!ctx.Model.Circles.TryAdd (circle.Id, circle))
				throw new ArgumentException ("Failed Add to Circles dictionary.");
		}

		public static void Add (this Context ctx, Arc arc)
		{
			if (!ctx.Model.Arcs.TryAdd (arc.Id, arc))
				throw new ArgumentException ("Failed Add to Arcs dictionary.");
		}

		public static void Add (this Context ctx, Text text)
		{
			if (!ctx.Model.Texts.TryAdd (text.Id, text))
				throw new ArgumentException ("Failed Add to Texts dictionary.");
		}

		#endregion

		#region Add Custom

		public static void Add (this Context ctx, Custom custom)
		{
			if (!ctx.Model.CustomElements.TryAdd (custom.Id, custom))
				throw new ArgumentException ("Failed Add to CustomElements dictionary.");
		}

		#endregion
	}

	#endregion

	#region Editor Parse Extensions

	public static class EditorParseExtensions
	{
		#region Parse Data

		public static void ParseData(this Context ctx, string data)
		{
			string type;
			var lines = Util.GetLines (data);
			int count = lines.Length;

			for(int i = 0; i < count; i++)
			{
				var args = Util.GetArgs (lines [i]);
				int length = args.Length;
				if (length < 2)
					throw new ArgumentException("Invalid number of arguments on line {0}.", i.ToString());

				type = args [0];

				if (Util.Compare (type, Constants.PinStyle) && length == 9) 
				{
					var style = ctx.ParsePinStyle (type, args);
					ctx.Add (style);
				}
				else if (Util.Compare (type, Constants.LineStyle) && length == 8) 
				{
					var style = ctx.ParseLineStyle (type, args);
					ctx.Add (style);
				}
				else if (Util.Compare (type, Constants.RectangleStyle) && length == 8) 
				{
					var style = ctx.ParseRectangleStyle (type, args);
					ctx.Add (style);
				}
				else if (Util.Compare (type, Constants.CircleStyle) && length == 8) 
				{
					var style = ctx.ParseCircleStyle (type, args);
					ctx.Add (style);
				}
				else if (Util.Compare (type, Constants.ArcStyle) && length == 8) 
				{
					var style = ctx.ParseArcStyle (type, args);
					ctx.Add (style);
				}
				else if (Util.Compare (type, Constants.TextStyle) && length == 11) 
				{
					var style = ctx.ParseTextStyle (type, args);
					ctx.Add (style);
				}
				else
				{
					if (Util.Compare (type, Constants.Pin) && length == 6) 
						ctx.PinArgs.Add (args);
					else if (Util.Compare (type, Constants.Line) && length == 5) 
						ctx.LineArgs.Add (args);
					else if (Util.Compare (type, Constants.Rectangle) && length == 5) 
						ctx.LineArgs.Add (args);
					else if (Util.Compare (type, Constants.Circle) && length == 5) 
						ctx.CircleArgs.Add (args);
					else if (Util.Compare (type, Constants.Arc) && length == 6) 
						ctx.ArcArgs.Add (args);
					else if (Util.Compare (type, Constants.Text) && length == 5) 
						ctx.TextArgs.Add (args);
					else if (type.Length >= 4 && type[0] == Constants.Reference)
						ctx.ReferenceArgs.Add (args);
					else if (type.Length >= 3 && length == 3)
						ctx.CustomArgs.Add (args);
					else
						throw new ArgumentException("Invalid type found on line number {0}.", i.ToString());
				}
			}
		}

		#endregion

		#region Parse Style

		public static PinStyle ParsePinStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);
			float radius = Util.GetFloat (args [8]);

			var renderer = ctx.PinRenderer();
			var style = new PinStyle(null,
			                         id, type, 
			                         colorA, colorR, colorG, colorB,
			                         isFilled,
			                         strokeWidth, 
			                         radius);

			renderer.Initialize (style);

			return style;
		}

		public static LineStyle ParseLineStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);

			var renderer = ctx.LineRenderer();
			var style = new LineStyle(renderer,
			                          id, type, 
			                          colorA, colorR, colorG, colorB,
			                          isFilled,
			                          strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		public static RectangleStyle ParseRectangleStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);

			var renderer = ctx.RectangleRenderer ();
			var style = new RectangleStyle(renderer,
			                               id, type, 
			                               colorA, colorR, colorG, colorB,
			                               isFilled,
			                               strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		public static CircleStyle ParseCircleStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);

			var renderer = ctx.CircleRenderer ();
			var style = new CircleStyle(renderer,
			                            id, type, 
			                            colorA, colorR, colorG, colorB,
			                            isFilled,
			                            strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		public static ArcStyle ParseArcStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);

			var renderer = ctx.ArcRenderer ();
			var style = new ArcStyle(renderer,
			                         id, type, 
			                         colorA, colorR, colorG, colorB,
			                         isFilled,
			                         strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		public static TextStyle ParseTextStyle(this Context ctx, string type, string[] args)
		{
			int id = Util.GetInt (args [1]);
			int colorA = Util.GetInt (args [2]);
			int colorR = Util.GetInt (args [3]);
			int colorG = Util.GetInt (args [4]);
			int colorB = Util.GetInt (args [5]);
			bool isFilled = Util.GetBool (args [6]);
			float strokeWidth = Util.GetFloat (args [7]);
			int hAlignment = Util.GetInt (args [8]);
			int vAlignment = Util.GetInt (args [9]);
			float size = Util.GetFloat (args [10]);

			var renderer = ctx.TextRenderer ();
			var style = new TextStyle(renderer, 
			                          id, type, 
			                          colorA, colorR, colorG, colorB,
			                          isFilled,
			                          strokeWidth,
			                          hAlignment, vAlignment,
			                          size);

			renderer.Initialize (style);

			return style;
		}

		#endregion

		#region Parse Primitive

		public static Pin ParsePin(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			float x = Util.GetFloat (args [3]);
			float y = Util.GetFloat (args [4]);
			bool isConnector = Util.GetBool (args [5]);

			var style = ctx.FindPinStyle (styleId);
			var pin = new Pin (id, type, style, x, y);

			return pin;
		}

		public static Line ParseLine(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			int startId = Util.GetInt (args [3]);
			int endId = Util.GetInt (args [4]);

			var style = ctx.FindLineStyle (styleId);
			var start =  ctx.FindPin(startId);
			var end = ctx.FindPin(endId);
			var line = new Line (id, type, style, start, end);

			return line;
		}

		public static Rectangle ParseRectangle(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			int topLeftId = Util.GetInt (args [3]);
			int bottomRightId = Util.GetInt (args [4]);

			var style = ctx.FindRectangleStyle (styleId);
			var topLeft = ctx.FindPin (topLeftId);
			var bottomRight = ctx.FindPin (bottomRightId);
			var rectangle = new Rectangle (id, type, style, topLeft, bottomRight);

			return rectangle;
		}

		public static Circle ParseCircle(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			int centerId = Util.GetInt (args [3]);
			int radiusId = Util.GetInt (args [4]);

			var style = ctx.FindCircleStyle (styleId);
			var center = ctx.FindPin (centerId);
			var radius = ctx.FindPin (radiusId);
			var circle = new Circle (id, type, style, center, radius);

			return circle;
		}

		public static Arc ParseArc(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			int startId = Util.GetInt (args [3]);
			int endId = Util.GetInt (args [4]);
			int centerId = Util.GetInt (args [5]);

			var style = ctx.FindArcStyle (styleId);
			var start = ctx.FindPin (startId);
			var end = ctx.FindPin (endId);
			var center = ctx.FindPin (centerId);
			var arc = new Arc (id, type, style, start, end, center);

			return arc;
		}

		public static Text ParseText(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);
			int styleId = Util.GetInt (args [2]);
			int positionId = Util.GetInt (args [3]);
			string textStr = args [4];
			bool isParam = Util.GetBool (args [5]);

			var style = ctx.FindTextStyle (styleId);
			var position = ctx.FindPin (positionId);
			var text = new Text (id, type, style, position);

			return text;
		}

		#endregion

		#region Parse Primitives
		
		public static void ParsePins(this Context ctx, IList<string[]> pins)
		{
			int count = pins.Count;
			for(int i = 0; i < count; i++)
			{
				var pin = ctx.ParsePin (pins [i]);
				ctx.Add (pin);
			}
		}

		public static void ParseLines(this Context ctx, IList<string[]> lines)
		{
			int count = lines.Count;
			for(int i = 0; i < count; i++)
			{
				var line = ctx.ParseLine (lines [i]);
				ctx.Add (line);
			}
		}

		public static void ParseRectangles(this Context ctx, IList<string[]> rectangles)
		{
			int count = rectangles.Count;
			for(int i = 0; i < count; i++)
			{
				var rectangle = ctx.ParseRectangle(rectangles [i]);
				ctx.Add (rectangle);
			}
		}

		public static void ParseCircles(this Context ctx, IList<string[]> circles)
		{
			int count = circles.Count;
			for(int i = 0; i < count; i++)
			{
				var circle = ctx.ParseCircle(circles [i]);
				ctx.Add (circle);
			}
		}

		public static void ParseArcs(this Context ctx, IList<string[]> arcs)
		{
			int count = arcs.Count;
			for(int i = 0; i < count; i++)
			{
				var arc = ctx.ParseArc(arcs [i]);
				ctx.Add (arc);
			}
		}

		public static void ParseTexts(this Context ctx, IList<string[]> texts)
		{
			int count = texts.Count;
			for(int i = 0; i < count; i++)
			{
				var text = ctx.ParseText(texts [i]);
				ctx.Add (text);
			}
		}

		#endregion

		#region Parse Custom

		public static Custom ParseCustom(this Context ctx, string[] args)
		{
			string type = args [0];
			int id = Util.GetInt (args [1]);

			int customArgsCount = args.Length - 2;
			if (customArgsCount % 2 != 0)
				throw new ArgumentException("Invalid format of custom element arguments.");

			// TODO: Create custom element renderer.
			//       Custom element renderer will render all referenced 
			//       primitives and passthrough custom elements to its renderers.
			//       Using this method each custom element renders only its primitives
			//       and lets all references to render by themeselft only
			//       passing location, width, height and rotation of custom element.
			//       Each referenced primitive has its renderer and we only
			//       need to define primitives renderes all other rendering
			//       requires only Pin X,Y matrix transformation and/or variable lookup.
			var custom = new Custom (id, type);

			for (int j = 0; j < customArgsCount; j += 2)
			{
				string argType = args [2 + j];
				int argId = Util.GetInt (args [2 + j + 1]);

				if (Util.Compare (argType, Constants.Pin)) 
				{
					var pin = ctx.FindPin (argId);
					custom.Primitives.Add (pin);
				}
				else if (Util.Compare (argType, Constants.Line)) 
				{
					var line = ctx.FindLine (argId);
					custom.Primitives.Add (line);
				}
				else if (Util.Compare (argType, Constants.Rectangle)) 
				{
					var rectangle = ctx.FindRectangle (argId);
					custom.Primitives.Add (rectangle);
				}
				else if (Util.Compare (argType, Constants.Circle)) 
				{
					var circle = ctx.FindCircle (argId);
					custom.Primitives.Add (circle);
				}
				else if (Util.Compare (argType, Constants.Arc)) 
				{
					var arc = ctx.FindArc (argId);
					custom.Primitives.Add (arc);
				}
				else if (Util.Compare (argType, Constants.Text)) 
				{
					var text = ctx.FindText (argId);
					custom.Primitives.Add (text);
				}
				else
				{
					if (argType.Length >= 4 && argType[0] == Constants.Reference)
					{
						// TODO: Store all references to custom types to resolve them in stage 4.
						throw new NotImplementedException ("References to custom types is not implemented.");
					}
					else
						throw new ArgumentException("Invalid custom element argument.");
				}
			}

			return custom;
		}

		#endregion

		#region Parse Customs

		public static void ParseCustoms(this Context ctx, IList<string[]> customs)
		{
			int count = customs.Count;
			for(int i = 0; i < count; i++)
			{
				var custom = ctx.ParseCustom (customs [i]);
				ctx.Add (custom);
			}
		}

		#endregion

		#region Resolve Custom Dependencies

		// TODO:

		#endregion

		#region Parse Reference

		// TODO:

		#endregion

		#region Parse References

		// TODO:

		#endregion
	}

	#endregion

	#region Editor

	public static class Editor
	{
		#region Stage 1

		private static void Stage1(Context ctx, string data)
		{
			ctx.ParseData (data);
		}

		#endregion

		#region Stage 2

		private static void Stage2(Context ctx)
		{
			// pins - parse before any other primitive type
			ctx.ParsePins (ctx.PinArgs);

			// lines
			ctx.ParseLines (ctx.LineArgs);

			// rectangles
			ctx.ParseRectangles (ctx.RectangleArgs);

			// circles
			ctx.ParseCircles (ctx.CircleArgs);

			// arcs
			ctx.ParseArcs (ctx.ArcArgs);

			// texts
			ctx.ParseTexts (ctx.TextArgs);
		}

		#endregion

		#region Stage 3

		private static void Stage3(Context ctx)
		{
			ctx.ParseCustoms (ctx.CustomArgs);
		}

		#endregion

		#region Stage 4

		private static void Stage4(Context ctx)
		{
			throw new NotImplementedException ("Parser stage 4 is not implemented.");
		}

		#endregion

		#region Stage 5

		private static void Stage5(Context ctx)
		{
			throw new NotImplementedException ("Parser stage 5 is not implemented.");
		}

		#endregion

		#region Parse

		public static Model Parse(string data, IRendererFactory factory)
		{
			var ctx = new Context (factory);

			//
			//  1st stage: get styles and sort other elements by type
			//

			Stage1 (ctx, data);

			//
			// 2nd stage: parse primitive types
			//

			Stage2 (ctx);

			//
			// 3rd stage: parse custom types
			//

			Stage3 (ctx);

			//
			//  4th stage: resolve custom elements reference dependencies
			//
			
			// TODO:
			// Each custom type may reference another custom type
			// create depnedences for each custom type
			// using topological sort order them by dependencies
			// than create custom types in that order of least depnedencies.
			// TODO:
			// Store all found dependecies in List<Element> Dependecies;
			// TODO: 
			// Currently custom types only can reference primtive types!

			Stage4 (ctx);

			//
			// 5th stage: parse references
			//

			Stage5 (ctx);
	
			return ctx.Model;
		}

		#endregion

		#region Generate
		/*
		private static StringBuilder sb = new StringBuilder();

		public static string Generate(ICollection<KeyValuePair<int, Element>> elements)
		{
			//var sb = new StringBuilder(); 
			sb.Length = 0;

			//var sw = System.Diagnostics.Stopwatch.StartNew ();

			foreach (var pair in elements) 
			{
				var element = pair.Value;

				if (element is Pin)
				{
					var pin = element as Pin;
					//string str = string.Format ("Pin;{0};{1};{2}", pin.Id, pin.X, pin.Y);
					//sb.AppendLine (str);

					sb.Append (Constants.Pin); 
					sb.Append (Constants.Separator);

					sb.Append (pin.Id);
					sb.Append (Constants.Separator);

					sb.Append (pin.X);
					sb.Append (Constants.Separator);

					sb.Append (pin.Y);
					sb.Append (Constants.NewLine);
				}
				else if (element is Wire) 
				{
					var wire = element as Wire;
					//string str = string.Format ("Wire;{0};{1};{2};{3};{4}", 
					//                            wire.Id, 
					//                            wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId, 
					//                            wire.Start.Id, 
					//                            wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId, 
					//                            wire.End.Id);
					//sb.AppendLine (str);

					sb.Append (Constants.Wire); 
					sb.Append (Constants.Separator);

					sb.Append (wire.Id);
					sb.Append (Constants.Separator);

					sb.Append (wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId);
					sb.Append (Constants.Separator);

					sb.Append (wire.Start.Id);
					sb.Append (Constants.Separator);

					sb.Append (wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId);
					sb.Append (Constants.Separator);

					sb.Append (wire.End.Id);
					sb.Append (Constants.NewLine);
				}
				else if (element is AndGate) 
				{
					var andGate = element as AndGate;
					//string str = string.Format ("AndGate;{0};{1};{2}", andGate.Id, andGate.X, andGate.Y);
					//sb.AppendLine (str);

					sb.Append (TagAndGate); 
					sb.Append (Constants.Separator);

					sb.Append (andGate.Id);
					sb.Append (Constants.Separator);

					sb.Append (andGate.X);
					sb.Append (Constants.Separator);

					sb.Append (andGate.Y);
					sb.Append (Constants.NewLine);
				}
				else if (element is OrGate) 
				{
					var orGate = element as OrGate;
					//string str = string.Format ("OrGate;{0};{1};{2}", orGate.Id, orGate.X, orGate.Y);
					//sb.AppendLine (str);

					sb.Append (TagOrGate); 
					sb.Append (Constants.Separator);

					sb.Append (orGate.Id);
					sb.Append (Constants.Separator);

					sb.Append (orGate.X);
					sb.Append (Constants.Separator);

					sb.Append (orGate.Y);
					sb.Append (Constants.NewLine);
				}
			}

			//sw.Stop ();
			//Console.WriteLine ("Generate: {0}ms", sw.Elapsed.TotalMilliseconds);

			return sb.ToString();
		}
		*/
		#endregion
	}

	#endregion
}
