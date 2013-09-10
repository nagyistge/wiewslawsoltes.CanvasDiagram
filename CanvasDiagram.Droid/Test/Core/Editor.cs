
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
	#region Editor

	public static class Editor
	{
		#region Util

		private static bool Compare(string a, string b)
		{
			return string.Compare (a, b, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		private static string[] GetLines(string model)
		{
			return model.Split (Constants.LineSeparators, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string[] GetArgs(string line)
		{
			return line.Split (Constants.ArgSeparators, StringSplitOptions.RemoveEmptyEntries);
		}

		private static bool GetBool(string flag)
		{
			return (int.Parse (flag) == 1) ? true : false;
		}

		private static int GetInt(string number)
		{
			return int.Parse (number);
		}

		private static float GetFloat(string number)
		{
			return float.Parse (number, System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
		}

		private static string GetString(float number)
		{
			return number.ToString (System.Globalization.CultureInfo.GetCultureInfo ("en-GB"));
		}

		#endregion

		#region Find Style

		private static PinStyle FindPinStyle (Context ctx, int styleId)
		{
			PinStyle style;
			if (!ctx.Model.PinStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find PinStyle.");
			return style;
		}

		private static LineStyle FindLineStyle (Context ctx, int styleId)
		{
			LineStyle style;
			if (!ctx.Model.LineStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find LineStyle.");
			return style;
		}

		private static RectangleStyle FindRectangleStyle (Context ctx, int styleId)
		{
			RectangleStyle style;
			if (!ctx.Model.RectangleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find RectangleStyle.");
			return style;
		}

		private static CircleStyle FindCircleStyle (Context ctx, int styleId)
		{
			CircleStyle style;
			if (!ctx.Model.CircleStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find CircleStyle.");
			return style;
		}

		private static ArcStyle FindArcStyle (Context ctx, int styleId)
		{
			ArcStyle style;
			if (!ctx.Model.ArcStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find ArcStyles.");
			return style;
		}

		private static TextStyle FindTextStyle (Context ctx, int styleId)
		{
			TextStyle style;
			if (!ctx.Model.TextStyles.TryGetValue (styleId, out style))
				throw new ArgumentException ("Failed to find TextStyles.");
			return style;
		}

		#endregion

		#region Find Primitive

		private static Pin FindPin (Context ctx, int id)
		{
			Pin pin;
			if (!ctx.Model.Pins.TryGetValue (id, out pin))
				throw new ArgumentException ("Failed to find Pin");
			return pin;
		}

		private static Line FindLine (Context ctx, int id)
		{
			Line line;
			if (!ctx.Model.Lines.TryGetValue (id, out line))
				throw new ArgumentException ("Failed to find Line");
			return line;
		}

		private static Rectangle FindRectangle (Context ctx, int id)
		{
			Rectangle rectangle;
			if (!ctx.Model.Rectangles.TryGetValue (id, out rectangle))
				throw new ArgumentException ("Failed to find Rectangle");
			return rectangle;
		}

		private static Circle FindCircle (Context ctx, int id)
		{
			Circle circle;
			if (!ctx.Model.Circles.TryGetValue (id, out circle))
				throw new ArgumentException ("Failed to find Circle");
			return circle;
		}

		private static Arc FindArc (Context ctx, int id)
		{
			Arc arc;
			if (!ctx.Model.Arcs.TryGetValue (id, out arc))
				throw new ArgumentException ("Failed to find Arc");
			return arc;
		}

		private static Text FindText (Context ctx, int id)
		{
			Text text;
			if (!ctx.Model.Texts.TryGetValue (id, out text))
				throw new ArgumentException ("Failed to find Text");
			return text;
		}

		#endregion

		#region Add Style
				
		private static void Add (Context ctx, PinStyle style)
		{
			if (!ctx.Model.PinStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to PinStyles dictionary.");
		}

		private static void Add (Context ctx, LineStyle style)
		{
			if (!ctx.Model.LineStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to LineStyles dictionary.");
		}

		private static void Add (Context ctx, RectangleStyle style)
		{
			if (!ctx.Model.RectangleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to RectangleStyles dictionary.");
		}

		private static void Add (Context ctx, CircleStyle style)
		{
			if (!ctx.Model.CircleStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to CircleStyles dictionary.");
		}

		private static void Add (Context ctx, ArcStyle style)
		{
			if (!ctx.Model.ArcStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to ArcStyles dictionary.");
		}

		private static void Add (Context ctx, TextStyle style)
		{
			if (!ctx.Model.TextStyles.TryAdd (style.Id, style))
				throw new ArgumentException ("Failed Add to TextStyles dictionary.");
		}

		#endregion

		#region Add Primitive

		private static void Add (Context ctx, Pin pin)
		{
			if (!ctx.Model.Pins.TryAdd (pin.Id, pin))
				throw new ArgumentException ("Failed Add to Pins dictionary.");
		}

		private static void Add (Context ctx, Line line)
		{
			if (!ctx.Model.Lines.TryAdd (line.Id, line))
				throw new ArgumentException ("Failed Add to Lines dictionary.");
		}

		private static void Add (Context ctx, Rectangle rectangle)
		{
			if (!ctx.Model.Rectangles.TryAdd (rectangle.Id, rectangle))
				throw new ArgumentException ("Failed Add to Rectangles dictionary.");
		}

		private static void Add (Context ctx, Circle circle)
		{
			if (!ctx.Model.Circles.TryAdd (circle.Id, circle))
				throw new ArgumentException ("Failed Add to Circles dictionary.");
		}

		private static void Add (Context ctx, Arc arc)
		{
			if (!ctx.Model.Arcs.TryAdd (arc.Id, arc))
				throw new ArgumentException ("Failed Add to Arcs dictionary.");
		}

		private static void Add (Context ctx, Text text)
		{
			if (!ctx.Model.Texts.TryAdd (text.Id, text))
				throw new ArgumentException ("Failed Add to Texts dictionary.");
		}

		#endregion

		#region Add Custom

		private static void Add (Context ctx, Custom custom)
		{
			if (!ctx.Model.CustomElements.TryAdd (custom.Id, custom))
				throw new ArgumentException ("Failed Add to CustomElements dictionary.");
		}

		#endregion

		#region Parse Style

		private static PinStyle ParsePinStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);
			float radius = GetFloat (args [8]);

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

		private static LineStyle ParseLineStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);

			var renderer = ctx.LineRenderer();
			var style = new LineStyle(renderer,
			                          id, type, 
			                          colorA, colorR, colorG, colorB,
			                          isFilled,
			                          strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		private static RectangleStyle ParseRectangleStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);

			var renderer = ctx.RectangleRenderer ();
			var style = new RectangleStyle(renderer,
			                               id, type, 
			                               colorA, colorR, colorG, colorB,
			                               isFilled,
			                               strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		private static CircleStyle ParseCircleStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);

			var renderer = ctx.CircleRenderer ();
			var style = new CircleStyle(renderer,
			                            id, type, 
			                            colorA, colorR, colorG, colorB,
			                            isFilled,
			                            strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		private static ArcStyle ParseArcStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);

			var renderer = ctx.ArcRenderer ();
			var style = new ArcStyle(renderer,
			                         id, type, 
			                         colorA, colorR, colorG, colorB,
			                         isFilled,
			                         strokeWidth);

			renderer.Initialize (style);

			return style;
		}

		private static TextStyle ParseTextStyle(Context ctx, string type, string[] args)
		{
			int id = int.Parse (args [1]);
			int colorA = GetInt (args [2]);
			int colorR = GetInt (args [3]);
			int colorG = GetInt (args [4]);
			int colorB = GetInt (args [5]);
			bool isFilled = GetBool (args [6]);
			float strokeWidth = GetFloat (args [7]);
			int hAlignment = GetInt (args [8]);
			int vAlignment = GetInt (args [9]);
			float size = GetFloat (args [10]);

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

		private static Pin ParsePin(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			float x = GetFloat (args [3]);
			float y = GetFloat (args [4]);
			bool isConnector = GetBool (args [5]);

			var style = FindPinStyle (ctx, styleId);
			var pin = new Pin (id, type, style, x, y);

			return pin;
		}

		private static Line ParseLine(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			int startId = GetInt (args [3]);
			int endId = GetInt (args [4]);

			var style = FindLineStyle (ctx, styleId);
			var start = FindPin (ctx, startId);
			var end = FindPin (ctx, endId);
			var line = new Line (id, type, style, start, end);

			return line;
		}

		private static Rectangle ParseRectangle(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			int topLeftId = GetInt (args [3]);
			int bottomRightId = GetInt (args [4]);

			var style = FindRectangleStyle (ctx, styleId);
			var topLeft = FindPin (ctx, topLeftId);
			var bottomRight = FindPin (ctx, bottomRightId);
			var rectangle = new Rectangle (id, type, style, topLeft, bottomRight);

			return rectangle;
		}

		private static Circle ParseCircle(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			int centerId = GetInt (args [3]);
			int radiusId = GetInt (args [4]);

			var style = FindCircleStyle (ctx, styleId);
			var center = FindPin (ctx, centerId);
			var radius = FindPin (ctx, radiusId);
			var circle = new Circle (id, type, style, center, radius);

			return circle;
		}

		private static Arc ParseArc(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			int startId = GetInt (args [3]);
			int endId = GetInt (args [4]);
			int centerId = GetInt (args [5]);

			var style = FindArcStyle (ctx, styleId);
			var start = FindPin (ctx, startId);
			var end = FindPin (ctx, endId);
			var center = FindPin (ctx, centerId);
			var arc = new Arc (id, type, style, start, end, center);

			return arc;
		}

		private static Text ParseText(Context ctx, string[] args)
		{
			string type = args [0];
			int id = GetInt (args [1]);
			int styleId = GetInt (args [2]);
			int positionId = GetInt (args [3]);
			string textStr = args [4];
			bool isParam = GetBool (args [5]);

			var style = FindTextStyle (ctx, styleId);
			var position = FindPin (ctx, positionId);
			var text = new Text (id, type, style, position);

			return text;
		}

		#endregion

		#region Stage 1

		private static void Stage1(Context ctx, string data)
		{
			string type;
			var lines = GetLines (data);
			int count = lines.Length;

			for(int i = 0; i < count; i++)
			{
				var args = GetArgs (lines [i]);
				int length = args.Length;
				if (length < 2)
					throw new ArgumentException("Invalid number of arguments on line {0}.", i.ToString());

				type = args [0];

				if (Compare (type, Constants.PinStyle) && length == 9) 
				{
					var style = ParsePinStyle (ctx, type, args);
					Add (ctx, style);
				}
				else if (Compare (type, Constants.LineStyle) && length == 8) 
				{
					var style = ParseLineStyle (ctx, type, args);
					Add (ctx, style);
				}
				else if (Compare (type, Constants.RectangleStyle) && length == 8) 
				{
					var style = ParseRectangleStyle (ctx, type, args);
					Add (ctx, style);
				}
				else if (Compare (type, Constants.CircleStyle) && length == 8) 
				{
					var style = ParseCircleStyle (ctx, type, args);
					Add (ctx, style);
				}
				else if (Compare (type, Constants.ArcStyle) && length == 8) 
				{
					var style = ParseArcStyle (ctx, type, args);
					Add (ctx, style);
				}
				else if (Compare (type, Constants.TextStyle) && length == 11) 
				{
					var style = ParseTextStyle (ctx, type, args);
					Add (ctx, style);
				}
				else
				{
					if (Compare (type, Constants.Pin) && length == 6) 
						ctx.PinArgs.Add (args);
					else if (Compare (type, Constants.Line) && length == 5) 
						ctx.LineArgs.Add (args);
					else if (Compare (type, Constants.Rectangle) && length == 5) 
						ctx.LineArgs.Add (args);
					else if (Compare (type, Constants.Circle) && length == 5) 
						ctx.CircleArgs.Add (args);
					else if (Compare (type, Constants.Arc) && length == 6) 
						ctx.ArcArgs.Add (args);
					else if (Compare (type, Constants.Text) && length == 5) 
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

		#region Stage 2

		private static void ParsePins(Context ctx, IList<string[]> pins)
		{
			int count = pins.Count;
			for(int i = 0; i < count; i++)
			{
				var pin = ParsePin (ctx, pins [i]);
				Add (ctx, pin);
			}
		}

		private static void ParseLines(Context ctx, IList<string[]> lines)
		{
			int count = lines.Count;
			for(int i = 0; i < count; i++)
			{
				var line = ParseLine (ctx, lines [i]);
				Add (ctx, line);
			}
		}

		private static void ParseRectangles(Context ctx, IList<string[]> rectangles)
		{
			int count = rectangles.Count;
			for(int i = 0; i < count; i++)
			{
				var rectangle = ParseRectangle(ctx, rectangles [i]);
				Add (ctx, rectangle);
			}
		}

		private static void ParseCircles(Context ctx, IList<string[]> circles)
		{
			int count = circles.Count;
			for(int i = 0; i < count; i++)
			{
				var circle = ParseCircle(ctx, circles [i]);
				Add (ctx, circle);
			}
		}

		private static void ParseArcs(Context ctx, IList<string[]> arcs)
		{
			int count = arcs.Count;
			for(int i = 0; i < count; i++)
			{
				var arc = ParseArc(ctx, arcs [i]);
				Add (ctx, arc);
			}
		}

		private static void ParseTexts(Context ctx, IList<string[]> texts)
		{
			int count = texts.Count;
			for(int i = 0; i < count; i++)
			{
				var text = ParseText(ctx, texts [i]);
				Add (ctx, text);
			}
		}

		private static void Stage2(Context ctx)
		{
			// pins - parse before any other primitive type
			ParsePins (ctx, ctx.PinArgs);

			// lines
			ParseLines (ctx,ctx.LineArgs);

			// rectangles
			ParseRectangles (ctx, ctx.RectangleArgs);

			// circles
			ParseCircles (ctx, ctx.CircleArgs);

			// arcs
			ParseArcs (ctx, ctx.ArcArgs);

			// texts
			ParseTexts (ctx, ctx.TextArgs);
		}

		#endregion

		#region Stage 3

		private static Custom ParseCustom(Context ctx, string[] args)
		{
			string type = args [0];
			int id = int.Parse (args [1]);

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
				int argId = GetInt (args [2 + j + 1]);

				if (Compare (argType, Constants.Pin)) 
				{
					var pin = FindPin (ctx, argId);
					custom.Primitives.Add (pin);
				}
				else if (Compare (argType, Constants.Line)) 
				{
					var line = FindLine (ctx, argId);
					custom.Primitives.Add (line);
				}
				else if (Compare (argType, Constants.Rectangle)) 
				{
					var rectangle = FindRectangle (ctx, argId);
					custom.Primitives.Add (rectangle);
				}
				else if (Compare (argType, Constants.Circle)) 
				{
					var circle = FindCircle (ctx, argId);
					custom.Primitives.Add (circle);
				}
				else if (Compare (argType, Constants.Arc)) 
				{
					var arc = FindArc (ctx, argId);
					custom.Primitives.Add (arc);
				}
				else if (Compare (argType, Constants.Text)) 
				{
					var text = FindText (ctx, argId);
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
		
		private static void ParseCustoms(Context ctx, IList<string[]> customs)
		{
			int count = customs.Count;
			for(int i = 0; i < count; i++)
			{
				var custom = ParseCustom (ctx, customs [i]);
				Add (ctx, custom);
			}
		}

		private static void Stage3(Context ctx)
		{
			ParseCustoms (ctx, ctx.CustomArgs);
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
