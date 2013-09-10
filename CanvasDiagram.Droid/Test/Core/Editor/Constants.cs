
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
	#region Constants

	public static class Constants
	{
		public const int StandaloneId = -1;

		public const char Separator = ';';
		public const char NewLine = '\n';

		public static char[] ArgSeparators = { ';', '\t', ' ' };
		public static char[] LineSeparators = { NewLine };

		public static char Reference = '!';

		public const string Pin = "P";
		public const string Line = "L";
		public const string Rectangle = "R";
		public const string Arc = "A";
		public const string Circle = "C";
		public const string Text = "T";
		public const string Variable = "V";

		public static string[] Primitives = { Pin, Line, Rectangle, Arc, Circle, Text };

		public const string PinStyle = "PS";
		public const string LineStyle = "LS";
		public const string RectangleStyle = "RS";
		public const string ArcStyle = "AS";
		public const string CircleStyle = "CS";
		public const string TextStyle = "TS";

		public static string[] Styles = { PinStyle, LineStyle, ArcStyle, CircleStyle, TextStyle };
	}

	#endregion
}
