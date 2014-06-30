using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using SQLite;

namespace CanvasDiagram.Droid
{
    public class Diagram
    {
        public Diagram() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Model { get; set; }
    }
}
