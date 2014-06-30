using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Graphics;

namespace CanvasDiagram.Droid
{
    public class OrGate : Element
    {
        public int Counter { get; set; }

        public OrGate(int id, float x, float y, int counter)
            : base()
        {
            Initialize(id, x, y, counter);
        }

        private void Initialize(int id, float x, float y, int counter)
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
