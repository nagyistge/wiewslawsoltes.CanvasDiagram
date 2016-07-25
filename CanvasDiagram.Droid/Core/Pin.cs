using System.Collections.Generic;
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
            Id = id;
            X = x;
            Y = y;
            Radius = radius;
            HitOffset = offset;
            Bounds = new RectF(
                x - radius - offset, 
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
            Bounds.Set(
                x - Radius - HitOffset, 
                y - Radius - HitOffset, 
                x + Radius + HitOffset, 
                y + Radius + HitOffset);
        }
    }
}
