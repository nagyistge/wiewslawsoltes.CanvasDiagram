using Android.Graphics;

namespace CanvasDiagram.Droid
{
    public class Wire : Element
    {
        public int StartParentId { get; set; }
        public int StartId { get; set; }
        public int EndParentId { get; set; }
        public int EndId { get; set; }
        public Pin Start { get; set; }
        public Pin End { get; set; }
        public float Radius { get; set; }
        public float HitOffset { get; set; }
        public RectF StartBounds { get; set; }
        public RectF EndBounds { get; set; }
        public PolygonF WireBounds { get; set; }

        public Wire(int id, Pin start, Pin end, float radius, float offset)
            : base()
        {
            Id = id;
            Start = start;
            End = end;

            StartParentId = -1;
            StartId = -1;
            EndParentId = -1;
            EndId = -1;

            Initialize(radius, offset);
        }

        public Wire(int id, int startParentId, int startId, int endParentId, int endId)
            : base()
        {
            Id = id;
            StartParentId = startParentId;
            StartId = startId;
            EndParentId = endParentId;
            EndId = endId;
        }

        public void Initialize(float radius, float offset)
        {
            Radius = radius;
            HitOffset = offset;

            float sx = Start.X;
            float sy = Start.Y;
            float ex = End.X;
            float ey = End.Y;

            // start point bounds
            StartBounds = new RectF(
                sx - radius - offset, 
                sy - radius - offset, 
                sx + radius + offset, 
                sy + radius + offset);

            // end point bounds
            EndBounds = new RectF(
                ex - radius - offset, 
                ey - radius - offset, 
                ex + radius + offset, 
                ey + radius + offset);

            // wire bounds
            int ps = 4;
            float[] px = new float[ps];
            float[] py = new float[ps];
            WireBounds = new PolygonF(px, py, ps);
            UpdateWireBounds(sx, sy, ex, ey);
        }

        public override void Update(float dx, float dy)
        {
            float radius = Radius;
            float offset = HitOffset;

            float sx = Start.X;
            float sy = Start.Y;
            float ex = End.X;
            float ey = End.Y;

            StartBounds.Set(
                sx - radius - offset, 
                sy - radius - offset, 
                sx + radius + offset, 
                sy + radius + offset);

            EndBounds.Set(
                ex - radius - offset, 
                ey - radius - offset, 
                ex + radius + offset, 
                ey + radius + offset);

            UpdateWireBounds(sx, sy, ex, ey);
        }

        private void UpdateWireBounds(float sx, float sy, float ex, float ey)
        {
            if ((ex > sx && ey < sy) 
                || (ex < sx && ey > sy))
            {
                WireBounds.SetX(0, StartBounds.Left);
                WireBounds.SetX(1, StartBounds.Right);
                WireBounds.SetX(2, EndBounds.Right);
                WireBounds.SetX(3, EndBounds.Left);

                WireBounds.SetY(0, StartBounds.Top);
                WireBounds.SetY(1, StartBounds.Bottom);
                WireBounds.SetY(2, EndBounds.Bottom);
                WireBounds.SetY(3, EndBounds.Top);
            }
            else
            {
                WireBounds.SetX(0, StartBounds.Left);
                WireBounds.SetX(1, StartBounds.Right);
                WireBounds.SetX(2, EndBounds.Right);
                WireBounds.SetX(3, EndBounds.Left);

                WireBounds.SetY(0, StartBounds.Bottom);
                WireBounds.SetY(1, StartBounds.Top);
                WireBounds.SetY(2, EndBounds.Top);
                WireBounds.SetY(3, EndBounds.Bottom);
            }
        }
    }
}
