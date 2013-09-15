
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
}
