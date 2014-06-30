using System;

namespace CanvasDiagram.Droid
{
    public class InputArgs
    {
        public int Action;
        public float X;
        public float Y;
        public int Index;
        public float X0;
        public float Y0;
        public float X1;
        public float Y1;
    }

    public static class InputActions
    {
        public const int None = 0;
        public const int Hitest = 2;
        public const int Move = 4;
        public const int StartZoom = 8;
        public const int Zoom = 16;
        public const int Merge = 32;
        public const int StartPan = 64;
    }
}
