#region References

using System;
using SQLite;

#endregion

namespace CanvasDiagram.Droid.Core
{
	#region Diagram

	public class Diagram
	{
		public Diagram ()
		{
		}

		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Title { get; set; }
		public string Model { get; set; }
	}

	#endregion
}
