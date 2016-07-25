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
    public abstract class Element
    {
        public int Id { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public RectF Bounds { get; set; }
        public bool ShowPins { get; set; }
        public List<Pin> Pins { get; set; }
        public bool IsSelected { get; set; }
        public Element Parent { get; set; }

        public virtual void Update(float dx, float dy)
        {
            float x = X + dx;
            float y = Y + dy;
            X = x; 
            Y = y;
            Bounds.Set(x, y, x + Width, y + Height);

            if (Pins != null)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    Pins[i].Update(dx, dy);
                }
            }
        }
    }
}
