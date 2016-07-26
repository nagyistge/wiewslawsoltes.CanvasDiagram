using System.Collections.Generic;
using Android.Graphics;

namespace CanvasDiagram.Droid
{
    public class OrGate : Element
    {
        public int Counter { get; set; }

        public OrGate(int id, float x, float y, int counter)
            : base()
        {
            float width = 30f;
            float height = 30f;
            float radius = 4f;
            float hitOffset = 3f;

            Id = id;
            Width = width;
            Height = height;
            X = x;
            Y = y;
            Bounds = new RectF(x, y, x + width, y + height);
            ShowPins = false;
            Pins = new List<Pin>();

            Pins.Add(new Pin(0, this, x + 0f, y + (height / 2f), radius, hitOffset)); // left
            Pins.Add(new Pin(1, this, x + width, y + (height / 2f), radius, hitOffset)); // right
            Pins.Add(new Pin(2, this, x + (width / 2f), y + 0f, radius, hitOffset)); // top
            Pins.Add(new Pin(3, this, x + (width / 2f), y + height, radius, hitOffset)); // bottom

            Counter = counter;
        }
    }
}
