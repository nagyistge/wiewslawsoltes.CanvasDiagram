#region References

using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

#endregion

namespace CanvasDiagram.Droid.Core
{
	#region DiagramRepository

	public class DiagramRepository
	{
		private SQLiteConnection conn;

		public DiagramRepository ()
		{
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			conn = new SQLiteConnection (System.IO.Path.Combine (folder, "diagrams.db"));
			conn.CreateTable<Diagram>();
		}

		public List<Diagram> GetAll()
		{
			return conn.Table<Diagram> ().OrderBy(x => x.Id).ToList();
		}

		public void RemoveAll()
		{
			foreach (var diagram in conn.Table<Diagram> ())
				conn.Delete (diagram);
		}

		public Diagram Get(int id)
		{
			return conn.Table<Diagram> ().FirstOrDefault (x => x.Id == id);
		}

		public int Save(Diagram diagram)
		{
			if (diagram.Id != 0) 
			{
				conn.Update (diagram);
				return diagram.Id;
			} 
			else 
			{
				return conn.Insert (diagram);
			}
		}

		public void Delete(Diagram diagram)
		{
			if (diagram.Id != 0)
				conn.Delete (diagram);
		}
	}

	#endregion
}

