
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
