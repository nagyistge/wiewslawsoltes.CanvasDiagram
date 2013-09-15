
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
}
