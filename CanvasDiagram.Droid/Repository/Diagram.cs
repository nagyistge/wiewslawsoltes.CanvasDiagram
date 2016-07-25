using SQLite;

namespace CanvasDiagram.Droid
{
    public class Diagram
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Model { get; set; }
    }
}
