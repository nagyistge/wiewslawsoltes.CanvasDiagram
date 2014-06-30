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
    public class Pin : Element
    {
        public float Radius { get; set; }
        public float HitOffset { get; set; }
        public List<Wire> Wires { get; set; }

        public Pin(int id, Element parent, float x, float y, float radius, float offset)
            : base()
        {
            Initialize(id, parent, x, y, radius, offset);
        }

        private void Initialize(int id, Element parent, float x, float y, float radius, float offset)
        {
            Id = id;
            X = x;
            Y = y;
            Radius = radius;
            HitOffset = offset;
            Bounds = new RectF(x - radius - offset, 
                y - radius - offset, 
                x + radius + offset, 
                y + radius + offset);
            Parent = parent;
            Wires = new List<Wire>();
        }

        public override void Update(float dx, float dy)
        {
            float x = X + dx;
            float y = Y + dy;
            X = x; 
            Y = y;
            Bounds.Set(x - Radius - HitOffset, 
                y - Radius - HitOffset, 
                x + Radius + HitOffset, 
                y + Radius + HitOffset);
        }
    }
}
