#region References
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SQLite;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.OS.Storage;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

#endregion

namespace CanvasDiagram.Droid
{
    #region Math

    public static class MathUtil
    {
        public static float LineDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static void LineMiddle(ref PointF point, float x1, float y1, float x2, float y2)
        {
            float x = x1 + x2;
            float y = y1 + y2;
            point.Set(x / 2f, y / 2f);
        }

        public static PointF ClosestPointOnLine(PointF a, PointF b, PointF p)
        {
            // en.wikipedia.org/wiki/Vector_projection
            return VectorAdd(VectorProject(VectorSubstract(p, a), VectorSubstract(b, a)), a);
        }

        public static float VectorDot(PointF a, PointF b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static PointF VectorMultiply(PointF a, float scalar)
        {
            return new PointF(a.X * scalar, a.Y * scalar);
        }

        public static PointF VectorProject(PointF a, PointF b)
        {
            return VectorMultiply(b, VectorDot(a, b) / VectorDot(b, b));
        }

        public static PointF VectorSubstract(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        public static PointF VectorAdd(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }
    }

    #endregion

    #region Core

    public class PolygonF
    {
        public int Sides { get { return polySides; } }

        private float[] polyY, polyX;
        private int polySides;

        public PolygonF(float[] px, float[] py, int ps)
        {
            polyX = px;
            polyY = py;
            polySides = ps;
        }

        public void SetX(int index, float x)
        {
            if (polyX != null && index >= 0 && index < polySides)
                polyX[index] = x;
        }

        public void SetY(int index, float y)
        {
            if (polyY != null && index >= 0 && index < polySides)
                polyY[index] = y;
        }

        public float GetX(int index)
        {
            if (polyX != null && index >= 0 && index < polySides)
                return polyX[index];
            return float.NaN;
        }

        public float GetY(int index)
        {
            if (polyY != null && index >= 0 && index < polySides)
                return polyY[index];
            return float.NaN;
        }

        public bool Contains(float x, float y)
        {
            if (polyX == null || polyY == null || polySides == 0)
                return false;

            bool c = false;
            int i, j = 0;

            for (i = 0, j = polySides - 1; i < polySides; j = i++)
            {
                if (((polyY[i] > y) != (polyY[j] > y))
                        && (x < (polyX[j] - polyX[i]) * (y - polyY[i]) / (polyY[j] - polyY[i]) + polyX[i]))
                {
                    c = !c;
                }
            }

            return c;
        }
    }

    public abstract class Element
    {
        #region Properties

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

        #endregion

        #region Update

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
                    Pins[i].Update(dx, dy);
            }
        }

        #endregion
    }

    public class Pin : Element
    {
        #region Properties

        public float Radius { get; set; }

        public float HitOffset { get; set; }

        public List<Wire> Wires { get; set; }

        #endregion

        #region Constructor

        public Pin(int id, Element parent, float x, float y, float radius, float offset)
            : base()
        {
            Initialize(id, parent, x, y, radius, offset);
        }

        #endregion

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

    public class Wire : Element
    {
        #region Properties

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

        #endregion

        #region Constructor

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

        #endregion

        public void Initialize(float radius, float offset)
        {
            Radius = radius;
            HitOffset = offset;

            float sx = Start.X;
            float sy = Start.Y;
            float ex = End.X;
            float ey = End.Y;

            // start point bounds
            StartBounds = new RectF(sx - radius - offset, 
                sy - radius - offset, 
                sx + radius + offset, 
                sy + radius + offset);

            // end point bounds
            EndBounds = new RectF(ex - radius - offset, 
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

            StartBounds.Set(sx - radius - offset, 
                sy - radius - offset, 
                sx + radius + offset, 
                sy + radius + offset);

            EndBounds.Set(ex - radius - offset, 
                ey - radius - offset, 
                ex + radius + offset, 
                ey + radius + offset);

            UpdateWireBounds(sx, sy, ex, ey);
        }

        private void UpdateWireBounds(float sx, float sy, float ex, float ey)
        {
            if ((ex > sx && ey < sy) || (ex < sx && ey > sy))
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

    public class AndGate : Element
    {
        #region Constructor

        public AndGate(int id, float x, float y)
            : base()
        {
            Initialize(id, x, y);
        }

        #endregion

        private void Initialize(int id, float x, float y)
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
        }
    }

    public class OrGate : Element
    {
        #region Properties

        public int Counter { get; set; }

        #endregion

        #region Constructor

        public OrGate(int id, float x, float y, int counter)
            : base()
        {
            Initialize(id, x, y, counter);
        }

        #endregion

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

    public static class Editor
    {
        #region Editor Constants

        public const int StandalonePinId = -1;

        public const char ModelSeparator = ';';
        public const string TagPin = "Pin";
        public const string TagWire = "Wire";
        public const string TagAndGate = "AndGate";
        public const string TagOrGate = "OrGate";
        public const char ModelNewLine = '\n';

        private static char[] ModelArgSeparators = new char[] { ';', '\t', ' ' };
        private static char[] ModelLineSeparators = new char[] { ModelNewLine };

        #endregion

        #region Parse

        public static void Parse(string model, ConcurrentDictionary<int, Element> elements)
        {
            string type = null;
            var lines = model.Split(ModelLineSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var element in lines)
            {
                var args = element.Split(ModelArgSeparators, StringSplitOptions.RemoveEmptyEntries);

                int length = args.Length;
                if (length < 2)
                    continue;

                type = args[0];

                if (string.Compare(type, "Pin", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var pin = new Pin(id, null, x, y, 4f, 3f);
                    elements.TryAdd(id, pin);
                }
                else if (string.Compare(type, "Wire", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 6)
                {
                    int id = int.Parse(args[1]);
                    int startParentId = int.Parse(args[2]);
                    int startId = int.Parse(args[3]);
                    int endParentId = int.Parse(args[4]);
                    int endtId = int.Parse(args[5]);

                    var wire = new Wire(id, startParentId, startId, endParentId, endtId);
                    elements.TryAdd(id, wire);
                }
                else if (string.Compare(type, "AndGate", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var andGate = new AndGate(id, x, y);
                    elements.TryAdd(id, andGate);
                }
                else if (string.Compare(type, "OrGate", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var orGate = new OrGate(id, x, y, 1);
                    elements.TryAdd(id, orGate);
                }
            }

            UpdateWireConnections(elements);
        }

        public static void UpdateWireConnections(ConcurrentDictionary<int, Element> elements)
        {
            var wires = elements.Where(x => x.Value is Wire);

            foreach (var pair in wires)
            {
                var wire = pair.Value as Wire;

                if (wire.StartParentId == StandalonePinId)
                {
                    Element start;
                    if (elements.TryGetValue(wire.StartId, out start))
                    {
                        var pin = start as Pin;
                        wire.Start = pin;
                        pin.Wires.Add(wire);
                    }
                }
                else
                {
                    Element parent;
                    if (elements.TryGetValue(wire.StartParentId, out parent))
                    {
                        var pin = parent.Pins.FirstOrDefault(x => x.Id == wire.StartId);
                        wire.Start = pin;
                        pin.Wires.Add(wire);
                    }
                }

                if (wire.EndParentId == StandalonePinId)
                {
                    Element end;
                    if (elements.TryGetValue(wire.EndId, out end))
                    {
                        var pin = end as Pin;
                        wire.End = pin;
                        pin.Wires.Add(wire);
                    }
                }
                else
                {
                    Element parent;
                    if (elements.TryGetValue(wire.EndParentId, out parent))
                    {
                        var pin = parent.Pins.FirstOrDefault(x => x.Id == wire.EndId);
                        wire.End = pin;
                        pin.Wires.Add(wire);
                    }
                }

                wire.Initialize(4f, 3f);
            }
        }

        #endregion

        #region Generate

        private static StringBuilder sb = new StringBuilder();

        public static string Generate(ICollection<KeyValuePair<int, Element>> elements)
        {
            //var sb = new StringBuilder(); 
            sb.Length = 0;

            //var sw = System.Diagnostics.Stopwatch.StartNew ();

            foreach (var pair in elements)
            {
                var element = pair.Value;

                if (element is Pin)
                {
                    var pin = element as Pin;
                    //string str = string.Format ("Pin;{0};{1};{2}", pin.Id, pin.X, pin.Y);
                    //sb.AppendLine (str);

                    sb.Append(TagPin); 
                    sb.Append(ModelSeparator);

                    sb.Append(pin.Id);
                    sb.Append(ModelSeparator);

                    sb.Append(pin.X);
                    sb.Append(ModelSeparator);

                    sb.Append(pin.Y);
                    sb.Append(ModelNewLine);
                }
                else if (element is Wire)
                {
                    var wire = element as Wire;
                    //string str = string.Format ("Wire;{0};{1};{2};{3};{4}", 
                    //                            wire.Id, 
                    //                            wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId, 
                    //                            wire.Start.Id, 
                    //                            wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId, 
                    //                            wire.End.Id);
                    //sb.AppendLine (str);

                    sb.Append(TagWire); 
                    sb.Append(ModelSeparator);

                    sb.Append(wire.Id);
                    sb.Append(ModelSeparator);

                    sb.Append(wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId);
                    sb.Append(ModelSeparator);

                    sb.Append(wire.Start.Id);
                    sb.Append(ModelSeparator);

                    sb.Append(wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId);
                    sb.Append(ModelSeparator);

                    sb.Append(wire.End.Id);
                    sb.Append(ModelNewLine);
                }
                else if (element is AndGate)
                {
                    var andGate = element as AndGate;
                    //string str = string.Format ("AndGate;{0};{1};{2}", andGate.Id, andGate.X, andGate.Y);
                    //sb.AppendLine (str);

                    sb.Append(TagAndGate); 
                    sb.Append(ModelSeparator);

                    sb.Append(andGate.Id);
                    sb.Append(ModelSeparator);

                    sb.Append(andGate.X);
                    sb.Append(ModelSeparator);

                    sb.Append(andGate.Y);
                    sb.Append(ModelNewLine);
                }
                else if (element is OrGate)
                {
                    var orGate = element as OrGate;
                    //string str = string.Format ("OrGate;{0};{1};{2}", orGate.Id, orGate.X, orGate.Y);
                    //sb.AppendLine (str);

                    sb.Append(TagOrGate); 
                    sb.Append(ModelSeparator);

                    sb.Append(orGate.Id);
                    sb.Append(ModelSeparator);

                    sb.Append(orGate.X);
                    sb.Append(ModelSeparator);

                    sb.Append(orGate.Y);
                    sb.Append(ModelNewLine);
                }
            }

            //sw.Stop ();
            //Console.WriteLine ("Generate: {0}ms", sw.Elapsed.TotalMilliseconds);

            return sb.ToString();
        }

        #endregion
    }

    #endregion

    #region Repository

    public class Diagram
    {
        public Diagram()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Model { get; set; }
    }

    public class DiagramRepository
    {
        private SQLiteConnection conn;

        public DiagramRepository()
        {
            string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            conn = new SQLiteConnection(System.IO.Path.Combine(folder, "diagrams.db"));
            conn.CreateTable<Diagram>();
        }

        public List<Diagram> GetAll()
        {
            return conn.Table<Diagram>().OrderBy(x => x.Id).ToList();
        }

        public void RemoveAll()
        {
            foreach (var diagram in conn.Table<Diagram> ())
                conn.Delete(diagram);
        }

        public Diagram Get(int id)
        {
            return conn.Table<Diagram>().FirstOrDefault(x => x.Id == id);
        }

        public int Save(Diagram diagram)
        {
            if (diagram.Id != 0)
            {
                conn.Update(diagram);
                return diagram.Id;
            }
            else
            {
                return conn.Insert(diagram);
            }
        }

        public void Delete(Diagram diagram)
        {
            if (diagram.Id != 0)
                conn.Delete(diagram);
        }
    }

    #endregion

    #region Threading

    public class DataHolder<T>
    {
        #region Properties

        public readonly object Sync = new object();

        public bool IsRunning { get; private set; }

        public T Data { get; private set; }

        public Action<T> Action { get; private set; }

        #endregion

        #region Constructor

        public DataHolder(Action<T> action, T data, bool isRunning)
        {
            Data = data;
            Action = action;
            IsRunning = isRunning;
        }

        #endregion

        #region Set

        public void SetAction(Action<T> action)
        {
            lock (Sync)
                Action = action;
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public bool SetData(T data, Action<T, T> copy, int timeout)
        {
            if (Monitor.TryEnter(Sync, timeout) == false)
                return false;

            copy(data, Data);

            Monitor.Pulse(Sync);
            Monitor.Exit(Sync);

            return true;
        }

        #endregion

        #region Run Loop

        public void Loop()
        {
            while (IsRunning)
            {
                lock (Sync)
                {
                    if (Action != null)
                        Action(Data);

                    Monitor.Wait(Sync);
                }
            }
        }

        #endregion
    }

    public class BackgroundService<T>
    {
        #region Fields

        private DataHolder<T> holder = null;
        private Thread thread = null;

        #endregion

        #region Lifecycle

        public void Start(Action<T> action, T data)
        {
            if (thread != null)
                return;

            holder = new DataHolder<T>(action, data, true);

            thread = new Thread(new ThreadStart(holder.Loop));
            thread.Start();
        }

        public void Stop()
        {
            if (thread == null)
                return;

            holder.SetRunning(false);
            lock (holder.Sync)
                Monitor.Pulse(holder.Sync);

            thread.Join();

            thread = null;
            holder = null;
        }

        #endregion

        #region Event Handler

        public bool HandleEvent(T data, Action<T, T> copy, int timeout)
        {
            return holder != null ? holder.SetData(data, copy, timeout) : false;
        }

        #endregion
    }

    #endregion

    #region Drawing

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

    public class DrawingView : SurfaceView, ISurfaceHolderCallback
    {
        #region Properties

        public DrawingService Service { get; set; }

        #endregion

        #region Fields

        public InputArgs Args = new InputArgs();

        #endregion

        #region Constructor

        public DrawingView(Context context)
            : base(context)
        {
            Initialize(context);
        }

        public DrawingView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize(context);
        }

        #endregion

        #region Initialize

        private void Initialize(Context context)
        {
            Holder.AddCallback(this);
            SetWillNotDraw(true);

            this.Touch += (sender, e) => HandleTouch(sender, e);

            Service = new DrawingService();
            Service.Initialize();
        }

        #endregion

        #region ISurfaceHolderCallback

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            Console.WriteLine("SurfaceChanged");

            Service.SurfaceWidth = width;
            Service.SurfaceHeight = height;

            Service.RedrawCanvas();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceCreated");

            Service.Start(this.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceDestroyed");

            Service.Stop();
        }

        #endregion

        #region Input Touch

        private static int GetPointerIndex(TouchEventArgs e)
        {
            int index = ((int)(e.Event.Action & MotionEventActions.PointerIndexMask)
                        >> (int)MotionEventActions.PointerIndexShift) == 0 ? 1 : 0;
            return index;
        }

        private void HandleTouch(object sender, TouchEventArgs e)
        {
            var action = e.Event.Action & MotionEventActions.Mask;
            int count = e.Event.PointerCount;

            Args.X = e.Event.GetX();
            Args.Y = e.Event.GetY();
            Args.Index = GetPointerIndex(e);
            Args.X0 = e.Event.GetX(0);
            Args.Y0 = e.Event.GetY(0);
            Args.X1 = count == 2 ? e.Event.GetX(1) : 0f;
            Args.Y1 = count == 2 ? e.Event.GetY(1) : 0f;

            if (count == 1 && action == MotionEventActions.Down)
                Args.Action = InputActions.Hitest;
            else if (count == 1 && action == MotionEventActions.Move)
                Args.Action = InputActions.Move;
            else if (count == 2 && action == MotionEventActions.PointerDown)
                Args.Action = InputActions.StartZoom;
            else if (count == 2 && action == MotionEventActions.Move)
                Args.Action = InputActions.Zoom;
            else if (action == MotionEventActions.Up)
                Args.Action = InputActions.Merge;
            else if (action == MotionEventActions.PointerUp)
                Args.Action = InputActions.StartPan;
            else
                Args.Action = InputActions.None;

            Service.RedrawCanvas(Args);
        }

        #endregion

        #region OnDraw

        protected override void OnDraw(Canvas canvas)
        {
        }

        #endregion
    }

    public class DrawingService
    {
        #region Properties

        public ConcurrentDictionary<int, Element> Elements { get; set; }

        public string CurrentModel { get; set; }

        public int SurfaceWidth { get; set; }

        public int SurfaceHeight { get; set; }

        #endregion

        #region Fields

        private BackgroundService<InputArgs> Service;
        private int FrameCount = 0;
        private InputArgs EmptyArgs = new InputArgs() { Action = InputActions.None };

        private ConcurrentStack<string> undoStack = new ConcurrentStack<string>();
        private ConcurrentStack<string> redoStack = new ConcurrentStack<string>();

        private Paint wirePaint;
        private Paint elementPaint;
        private Paint pinPaint;
        //private Paint pagePaint;
        private Paint gridPaint;
        private Paint boundsPaint;
        private Paint textElementPaint;
        //private Paint textIOPaint;
        private Paint test;

        private Color colorBackground = Color.Argb(255, 221, 221, 221);
        // #FFDDDDDD
        private Color coloPage = Color.Argb(255, 116, 116, 116);
        // #FF747474
        private Color colorGrid = Color.Argb(255, 180, 180, 180);
        // #FFB4B4B4
        private Color colorElement = Color.Argb(255, 0, 0, 0);
        // Black, #FF000000
        private Color colorSelected = Color.Argb(255, 255, 20, 147);
        // DeepPink, #FFFF1493
        private Color colorBounds = Color.Argb(128, 255, 255, 0);
        // transparent Yellow

        private float wireStrokeWidth = 2f;
        private float elementStrokeWidth = 2f;
        //private float pageStrokeWidth = 1f;
        private float gridStrokeWidth = 1f;
        private float pinStrokeWidth = 1f;

        private RectF widgetBounds = new RectF();

        private Wire currentWire = null;
        private Element currentElement = null;
        private Pin startPin = null;
        private Pin endPin = null;

        private float previousX = 0f;
        private float previousY = 0f;

        private Matrix matrix = new Matrix();
        private Matrix savedMatrix = new Matrix();
        private float[] matrixValues = new float[9];
        private PointF start = new PointF(0f, 0f);

        private float minPinchToZoomDistance = 10f;
        private float previousDist = 0f;

        public float zoom = 1f;
        public PointF translate = null;
        public PointF middle = null;

        private bool moveCurrentElementStarted = false;

        private int nextId = 0;

        #endregion

        #region Lifecycle

        public void Start(ISurfaceHolder surfaceHolder)
        {
            try
            {
                if (Service == null)
                    Service = new BackgroundService<InputArgs>();

                ISurfaceHolder holder = surfaceHolder;
                Canvas canvas = null;

                Service.Start((data) =>
                    {
                        canvas = null;

                        try
                        {
                            HandleInput(data);
                            canvas = holder.LockCanvas(null);
                            this.DrawDiagram(canvas);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                        }
                        finally
                        {
                            if (canvas != null)
                                holder.UnlockCanvasAndPost(canvas);
                        }
                    },
                    new InputArgs());
            }
            catch (Exception ex)
            { 
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Stop()
        {
            try
            {
                if (Service != null)
                    Service.Stop();
            }
            catch (Exception ex)
            { 
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Id

        public int GetNextId()
        { 
            return nextId++; 
        }

        public void SetNextId(int id)
        {
            nextId = id;
        }

        public void UpdateNextId()
        {
            var nextId = this.Elements.Count == 0 ? 0 : this.Elements.Max(x => x.Value.Id) + 1;
            this.SetNextId(nextId);
        }

        #endregion

        #region Initialize

        public void Initialize()
        {
            CreatePaints();
            Elements = new ConcurrentDictionary<int, Element>();
            translate = new PointF(0f, 0f);
            middle = new PointF(0f, 0f);
            ResetZoom();
        }

        private void CreatePaints()
        {
            wirePaint = new Paint()
            {
                Color = colorElement,
                AntiAlias = true,
                StrokeWidth = wireStrokeWidth,
                StrokeCap = Paint.Cap.Square
            };

            elementPaint = new Paint()
            {
                Color = colorElement,
                AntiAlias = true,
                StrokeWidth = elementStrokeWidth,
                StrokeCap = Paint.Cap.Square
            };

            pinPaint = new Paint()
            {
                Color = colorElement,
                AntiAlias = true,
                StrokeWidth = pinStrokeWidth,
                StrokeCap = Paint.Cap.Butt
            };

            textElementPaint = new Paint()
            {
                Color = colorElement,
                AntiAlias = true,
                StrokeWidth = 1f,
                TextAlign = Paint.Align.Center,
                TextSize = 14f,
                SubpixelText = true
            };

            gridPaint = new Paint()
            {
                Color = colorGrid,
                AntiAlias = true,
                StrokeWidth = gridStrokeWidth,
                StrokeCap = Paint.Cap.Butt
            };
            gridPaint.SetStyle(Paint.Style.Stroke);

            boundsPaint = new Paint()
            {
                Color = colorBounds,
                AntiAlias = true,
                StrokeWidth = pinStrokeWidth,
                StrokeCap = Paint.Cap.Butt
            };
            boundsPaint.SetStyle(Paint.Style.Stroke);

            test = new Paint()
            {
                Color = Color.Argb(255, 255, 0, 0),
                AntiAlias = true,
                StrokeWidth = 1f,
                StrokeCap = Paint.Cap.Butt
            };
            test.SetStyle(Paint.Style.Stroke);
        }

        #endregion

        #region Input HitTest

        private void ResetStartAndEndPin()
        {
            if (startPin != null)
            {
                startPin.IsSelected = false;
                startPin = null;
            }

            if (endPin != null)
            {
                endPin.IsSelected = false;
                endPin = null;
            }
        }

        private void SetWireSelection(Wire wire, bool selected, Wire found)
        {
            wire.IsSelected = selected;

            bool setStart = found == null ? true : (wire.Start != found.Start.Parent && wire.Start.Parent != found.End.Parent);
            bool setEnd = found == null ? true : (wire.End != found.Start.Parent && wire.End.Parent != found.End.Parent);

            if (setStart == true)
                wire.Start.IsSelected = selected;

            if (setEnd == true)
                wire.End.IsSelected = selected;

            bool setStartParent = found == null ? true : (wire.Start.Parent != found.Start.Parent && wire.Start.Parent != found.End.Parent);
            bool setEndParent = found == null ? true : (wire.End.Parent != found.Start.Parent && wire.End.Parent != found.End.Parent);

            if (wire.Start.Parent != null && setStartParent == true)
                wire.Start.Parent.ShowPins = selected;

            if (wire.End.Parent != null && setEndParent == true)
                wire.End.Parent.ShowPins = selected;
        }

        private bool HitTestWire(float x, float y, bool deselect)
        {
            Wire found = null;
            bool haveWire = false;

            // find wire that contains touch point
            foreach (var pair in Elements.Where(_ => _.Value is Wire))
            {
                var wire = pair.Value as Wire;

                bool startBounds = wire.StartBounds.Contains(x, y) == true;
                bool endBounds = wire.EndBounds.Contains(x, y) == true;
                bool wireBounds = wire.WireBounds.Contains(x, y) == true;

                if (found == null && (wireBounds == true && !startBounds && !endBounds))
                {
                    if (wire.IsSelected == false)
                    {
                        haveWire = true;
                        found = wire;
                        // select wire
                        SetWireSelection(wire, true, null);
                    }
                    else
                    {
                        currentWire = null;
                        SetWireSelection(wire, false, null);
                    }
                }
                else
                {
                    // deselect wire
                    if (deselect == true)
                        SetWireSelection(wire, false, found);
                }
            }

            if (found != null)
                currentWire = found;

            return haveWire;
        }

        private bool HitTestPin(float x, float y)
        {
            bool havePin = false;

            foreach (var pin in currentElement.Pins)
            {
                if (pin.Bounds.Contains(x, y) == true)
                {
                    if (startPin == null)
                    {
                        // select as start pin
                        startPin = pin;
                        startPin.IsSelected = true;
                        havePin = true;
                        break;
                    }
                    else if (endPin == null)
                    {
                        // check if can be connected as end pin
                        if (pin != startPin && pin.Parent != startPin.Parent)
                        {
                            endPin = pin;
                            endPin.IsSelected = true;
                        }

                        // reset start pin
                        if (pin == startPin)
                        {
                            startPin.IsSelected = false;
                            startPin = null;
                        }

                        havePin = true;
                        break;
                    }
                    else
                    {
                        ResetStartAndEndPin();

                        havePin = true;
                        break;
                    }
                }
            }

            return havePin;
        }

        private bool HitTestElement(float x, float y)
        {
            RectF rect = null;
            bool haveElement = false;

            // find element that contains touch point
            foreach (var pair in Elements.Where(_ => !(_.Value is Wire)))
            {
                var element = pair.Value;

                if (rect == null && element.Bounds.Contains(x, y) == true)
                {
                    haveElement = true;
                    currentElement = element;
                    element.ShowPins = true;
                    rect = element.Bounds;
                }
                else
                {
                    // hide element pins except for parent of start pin
                    if (!(startPin != null && endPin == null && startPin.Parent == element))
                        element.ShowPins = false;
                }
            }

            return haveElement;
        }

        #endregion

        #region Input Move

        private void MoveCurrentElement(float x, float y)
        {
            float scaledX = (x - translate.X) / zoom;
            float scaledY = (y - translate.Y) / zoom;

            // calculate move delta
            float dx = scaledX - previousX;
            float dy = scaledY - previousY;

            currentElement.Update(dx, dy);

            // update previous position
            previousX = scaledX;
            previousY = scaledY;

            // update wire bounds
            var pins = currentElement.Pins;
            if (pins != null)
                UpdatePinWires(pins, dx, dy);
            else if (currentElement is Pin)
                UpdateWires((currentElement as Pin).Wires, dx, dy);
        }

        public void FinishCurrentElement(float x, float y)
        {
            // try to merge wire to current pin
            if (currentElement is Pin)
            {
                bool mergedWires = TryToMergeWire(x, y, currentElement as Pin);

                if (mergedWires == false)
                {
                    // TODO:
                }
            }

            moveCurrentElementStarted = false;
        }

        #endregion

        #region Input Pan

        public void SetPanStart(float x, float y)
        {
            savedMatrix.Set(matrix);
            start.Set(x, y);
        }

        private void PanCanvas(float x, float y)
        {
            matrix.Set(savedMatrix);
            matrix.PostTranslate(x - start.X, y - start.Y);
            matrix.GetValues(matrixValues);

            translate.Set(matrixValues[Matrix.MtransX], matrixValues[Matrix.MtransY]);
        }

        #endregion

        #region Input Zoom

        public void PinchToZoom(float x0, float y0, float x1, float y1)
        {
            float currentDist = MathUtil.LineDistance(x0, y0, x1, y1);

            if (currentDist > minPinchToZoomDistance)
            {
                float scale = currentDist / previousDist;

                matrix.Set(savedMatrix);
                matrix.PostScale(scale, scale, middle.X, middle.Y);
                matrix.GetValues(matrixValues);

                zoom = matrixValues[Matrix.MscaleX];
                translate.Set(matrixValues[Matrix.MtransX], matrixValues[Matrix.MtransY]);
            }
        }

        public void StartPinchToZoom(float x0, float y0, float x1, float y1)
        {
            // update previous distance
            previousDist = MathUtil.LineDistance(x0, y0, x1, y1);

            if (previousDist > minPinchToZoomDistance)
            {
                savedMatrix.Set(matrix);

                MathUtil.LineMiddle(ref middle, x0, y0, x1, y1);
            }
        }

        #endregion

        #region Wires

        public static void UpdatePinWires(List<Pin> pins, float dx, float dy)
        {
            for (int i = 0; i < pins.Count; i++)
                UpdateWires(pins[i].Wires, dx, dy);
        }

        public static void UpdateWires(List<Wire> wires, float dx, float dy)
        {
            for (int j = 0; j < wires.Count; j++)
                wires[j].Update(dx, dy);
        }

        public static void SwapWireStart(Wire wire, Pin start, Pin pin)
        {
            start.Wires.Remove(wire);
            pin.Wires.Add(wire);
            wire.Start = pin;
            wire.Update(0f, 0f);
        }

        public static void SwapWireEnd(Wire wire, Pin end, Pin pin)
        {
            end.Wires.Remove(wire);
            pin.Wires.Add(wire);
            wire.End = pin;
            wire.Update(0f, 0f);
        }

        private void SplitWire(Wire wire, float x, float y, bool findClosest)
        {
            // find closest point on line
            var start = wire.Start;
            var end = wire.End;
            var a = new PointF(start.X, start.Y);
            var b = new PointF(end.X, end.Y);
            var p = new PointF(x, y);
            var insert = findClosest ? MathUtil.ClosestPointOnLine(a, b, p) : p;

            // create standalone pin
            var pin = InsertPin(insert.X, insert.Y, false);

            // set current wire end to standalone pin
            SwapWireEnd(wire, end, pin);

            // connect start pin to standalone pin
            InsertWire(startPin, pin, false);

            // connect current wire old end to standalone pin
            InsertWire(pin, end, false);

            // reset start pin
            startPin.IsSelected = false;
            startPin = null;

            // set standalone pin as current element
            currentElement = pin;
            currentElement.ShowPins = true;
        }

        private bool TryToDetachWire(float x, float y)
        {
            float scaledX = (x - translate.X) / zoom;
            float scaledY = (y - translate.Y) / zoom;

            // find wire that contains touch point
            Wire wireToDetach = null;
            Pin startToDetach = null;
            Pin endToDetach = null;

            // try to detach wire Pin from current element
            foreach (var pair in Elements.Where (_ => _.Value is Wire))
            {
                var wire = pair.Value as Wire;
                bool startBounds = wire.StartBounds.Contains(scaledX, scaledY) == true;
                bool endBounds = wire.EndBounds.Contains(scaledX, scaledY) == true;
                bool wireBounds = wire.WireBounds.Contains(scaledX, scaledY) == true;

                // check for wire start pin
                if (startBounds == true)
                {
                    wireToDetach = wire;
                    startToDetach = wire.Start;
                    break;
                }

                // check for wire end pin
                if (endBounds == true)
                {
                    wireToDetach = wire;
                    endToDetach = wire.End;
                    break;
                }
            }

            // detach wire start
            if (wireToDetach != null && startToDetach != null && startToDetach.Parent != null)
            {
                // create standalone pin
                var pin = InsertPin(scaledX, scaledY, false);

                // set detached wire start to standalone pin
                SwapWireStart(wireToDetach, startToDetach, pin);

                // reset start pin
                startToDetach.IsSelected = false;

                // set standalone pin as current element
                currentElement = pin;
                currentElement.ShowPins = true;

                UpdateWires(pin.Wires, 0f, 0f);

                ResetStartAndEndPin();

                return true;
            }

            // detach wire end
            if (wireToDetach != null && endToDetach != null && endToDetach.Parent != null)
            {
                // create standalone pin
                var pin = InsertPin(scaledX, scaledY, false);

                // set detached wire end to standalone pin
                SwapWireEnd(wireToDetach, endToDetach, pin);

                // reset start pin
                endToDetach.IsSelected = false;

                // set standalone pin as current element
                currentElement = pin;
                currentElement.ShowPins = true;

                UpdateWires(pin.Wires, 0f, 0f);

                ResetStartAndEndPin();

                return true;
            }

            return false;
        }

        private void MergeWireStart(Wire wire, Pin start, Pin pin)
        {
            // set merged wire start to pin
            SwapWireStart(wire, start, pin);

            // reset start pin
            start.IsSelected = false;

            // remove start from Elements
            Element value;
            Elements.TryRemove(start.Id, out value);

            //Console.WriteLine ("MergeWireStart: Remove Start.Id {0}", start.Id);
        }

        private void MergeWireEnd(Wire wire, Pin end, Pin pin)
        {
            // set merged wire end to pin
            SwapWireEnd(wire, end, pin);

            // reset start pin
            end.IsSelected = false;

            // remove end from Elements
            Element value;
            Elements.TryRemove(end.Id, out value);

            //Console.WriteLine ("MergeWireEnd: Remove End.Id {0}", end.Id);
        }

        private void UpdateMergedPinWires(Pin pin, Wire original, Pin old)
        {
            var wires = old.Wires.ToList();

            for (int i = 0; i < wires.Count; i++)
            {
                var wire = wires[i];
                if (wire == original)
                {
                    //Console.WriteLine ("UpdateMergedPinWires: wire == original");
                    continue;
                }

                if (wire.Start == old)
                {
                    MergeWireStart(wire, old, pin);

                    // if pin is wire end than wire is removed
                    if (pin == wire.End)
                    {
                        if (pin.Wires.Count == 1)
                        {
                            pin.Wires.Remove(wire);
                        }

                        Element value;
                        Elements.TryRemove(wire.Id, out value);

                        wire.Start.Wires.Remove(wire);
                        wire.End.Wires.Remove(wire);

                        //Console.WriteLine ("UpdateMergedPinWires: wire.Start==old, Pin.Id {0} OriginalWire.Id {1} Remove Wire.Id {2}", pin.Id, original.Id, wire.Id);
                    }
                }

                if (wire.End == old)
                {
                    MergeWireEnd(wire, old, pin);

                    // if pin is wire start than wire is removed
                    if (pin == wire.Start)
                    {
                        if (pin.Wires.Count == 1)
                        {
                            pin.Wires.Remove(wire);
                        }

                        Element value;
                        Elements.TryRemove(wire.Id, out value);

                        wire.Start.Wires.Remove(wire);
                        wire.End.Wires.Remove(wire);

                        //Console.WriteLine ("UpdateMergedPinWires: wire.End==old, Pin.Id {0} OriginalWire.Id {1} Remove Wire.Id {2}", pin.Id, original.Id, wire.Id);
                    }
                }
            }
        }

        private bool TryToMergeWire(float x, float y, Pin pin)
        {
            float scaledX = (x - translate.X) / zoom;
            float scaledY = (y - translate.Y) / zoom;

            // find wire that contains touch point
            Wire wireToMerge = null;
            Pin startToMerge = null;
            Pin endToMerge = null;

            // try to merge wire Pin to current element Pin
            foreach (var pair in Elements.Where (_ => _.Value is Wire))
            {
                var wire = pair.Value as Wire;
                bool startBounds = wire.StartBounds.Contains(scaledX, scaledY) == true;
                bool endBounds = wire.EndBounds.Contains(scaledX, scaledY) == true;
                bool wireBounds = wire.WireBounds.Contains(scaledX, scaledY) == true;

                // check for wire start pin
                // exlude if wire start is same as pin to merge
                if (startBounds == true && wire.Start != pin)
                {
                    wireToMerge = wire;
                    startToMerge = wire.Start;
                    break;
                }

                // check for wire end pin
                // exlude if wire end is same as pin to merge
                if (endBounds == true && wire.End != pin)
                {
                    wireToMerge = wire;
                    endToMerge = wire.End;
                    break;
                }
            }

            // merge wire start
            if (wireToMerge != null && startToMerge != null && startToMerge.Parent == null)
            {
                MergeWireStart(wireToMerge, startToMerge, pin);

                // if pin is wire end than wire is removed
                if (pin == wireToMerge.End)
                {
                    Element value;
                    Elements.TryRemove(wireToMerge.Id, out value);

                    wireToMerge.Start.Wires.Remove(wireToMerge);
                    wireToMerge.End.Wires.Remove(wireToMerge);

                    //Console.WriteLine ("TryToMergeWire: pin==wireToMerge.End, Pin.Id {0} Remove wireToMerge.Id", pin.Id, wireToMerge.Id);
                }

                // check for other end Wires connections to be merged
                UpdateMergedPinWires(pin, wireToMerge, startToMerge);

                // if merged start does not have any connected wires than need to be removed
                if (startToMerge.Wires.Count == 0)
                {
                    Element value;
                    Elements.TryRemove(startToMerge.Id, out value);

                    //Console.WriteLine ("TryToMergeWire: startToMerge.Wires.Count==0, Pin.Id {0} Remove startToMerge.Id", pin.Id, startToMerge.Id);
                }

                // if pin does not have any connected wires than need to be removed
                if (pin.Wires.Count == 0)
                {
                    Element value;
                    Elements.TryRemove(pin.Id, out value);

                    //Console.WriteLine ("TryToMergeWire: pin.Wires.Count==0, Pin.Id {0} Remove pin.Id", pin.Id);
                }
                    // otherwise update all wires connected to pin
                    else
                    UpdateWires(pin.Wires, 0f, 0f);

                ResetStartAndEndPin();

                return true;
            }

            // merge wire end
            if (wireToMerge != null && endToMerge != null && endToMerge.Parent == null)
            {
                MergeWireEnd(wireToMerge, endToMerge, pin);

                // if pin is wire start than wire is removed
                if (pin == wireToMerge.Start)
                {
                    Element value;
                    Elements.TryRemove(wireToMerge.Id, out value);

                    wireToMerge.Start.Wires.Remove(wireToMerge);
                    wireToMerge.End.Wires.Remove(wireToMerge);

                    //Console.WriteLine ("TryToMergeWire: pin==wireToMerge.Start, Pin.Id {0} Remove wireToMerge.Id", pin.Id, wireToMerge.Id);
                }

                // check for other end Wires connections to be merged
                UpdateMergedPinWires(pin, wireToMerge, endToMerge);

                // if merged end does not have any connected wires than need to be removed
                if (endToMerge.Wires.Count == 0)
                {
                    Element value;
                    Elements.TryRemove(endToMerge.Id, out value);

                    //Console.WriteLine ("TryToMergeWire: endToMerge.Wires.Count==0, Pin.Id {0} Remove endToMerge.Id", pin.Id, endToMerge.Id);
                }

                // if pin does not have any connected wires than need to be removed
                if (pin.Wires.Count == 0)
                {
                    Element value;
                    Elements.TryRemove(pin.Id, out value);

                    //Console.WriteLine ("TryToMergeWire: pin.Wires.Count==0, Pin.Id {0} Remove pin.Id", pin.Id);
                }
                    // otherwise update all wires connected to pin
                    else
                    UpdateWires(pin.Wires, 0f, 0f);

                ResetStartAndEndPin();

                return true;
            }

            return false;
        }

        #endregion

        #region Input Touch

        public void HandleOneInputDown(float x, float y)
        {
            float scaledX = (x - translate.X) / zoom;
            float scaledY = (y - translate.Y) / zoom;
            bool haveWire = false;
            bool havePin = false;

            // update previous position
            previousX = scaledX;
            previousY = scaledY;

            // set pan start position
            SetPanStart(x, y);

            // check for wires
            // - if no current element selected
            // - if have start pin to split wires
            if (currentElement == null)
            {
                haveWire = HitTestWire(scaledX, scaledY, true);
            }
            else if (currentElement != null && (startPin != null && endPin == null))
            {
                haveWire = HitTestWire(scaledX, scaledY, false);
            }

            // check for current element pins
            if (currentElement != null && currentElement.ShowPins == true &&
                !(currentElement is Pin) &&
                (startPin == null || endPin == null) &&
                haveWire == false)
            {
                havePin = HitTestPin(scaledX, scaledY);
            }

            if (startPin != null && endPin != null &&
                havePin == true && haveWire == false)
            {
                Snapshot();
                InsertWire(startPin, endPin, false);
            }
            else if (havePin == false && haveWire == true)
            {
                // split wire
                if (startPin != null && endPin == null)
                {
                    Snapshot();

                    // deselect current wire
                    SetWireSelection(currentWire, false, null);

                    // split current wire
                    SplitWire(currentWire, scaledX, scaledY, true);
                }
            }
            else if (havePin == false && haveWire == false)
            {
                bool haveElement = HitTestElement(scaledX, scaledY);

                // reset current element and start&end pins
                // if current element is not present and do not have start pin
                if (haveElement == false && !(startPin != null && endPin == null))
                {
                    ResetStartAndEndPin();
                    currentElement = null;
                }
                else if (haveElement == false && startPin != null && endPin == null)
                {
                    Snapshot();

                    // create standalone pin
                    var pin = InsertPin(scaledX, scaledY, false);
                    InsertWire(startPin, pin, false);

                    // reset previous start pin
                    startPin.IsSelected = false;

                    // set new start pin
                    startPin = pin;
                    startPin.IsSelected = true;
                    currentElement = pin;
                }
                    // reset start and end pins if have new element selected
                    // and start and end pin has beed already connected
                    else if (haveElement == true && startPin != null && endPin != null)
                {
                    ResetStartAndEndPin();
                }
            }
        }

        public void HandleOneInputMove(float x, float y)
        {
            if (currentElement != null)
            {
                if (moveCurrentElementStarted == false)
                {
                    moveCurrentElementStarted = true;

                    Snapshot();

                    if (!(currentElement is Pin))
                    {
                        //if (TryToDetachWire (x, y) == true) 
                        //  Snapshot ();
                        TryToDetachWire(x, y);
                    }
                }

                MoveCurrentElement(x, y);
            }
            else
            {
                PanCanvas(x, y);
            }
        }

        #endregion

        #region Input

        public void HandleInput(InputArgs args)
        {
            int action = args.Action;

            if (action == InputActions.Hitest)
            {
                HandleOneInputDown(args.X, args.Y);
            }
            else if (action == InputActions.Move)
            {
                HandleOneInputMove(args.X, args.Y);
            }
            else if (action == InputActions.StartZoom)
            {
                StartPinchToZoom(args.X0, args.Y0, args.X1, args.Y1);
            }
            else if (action == InputActions.Zoom)
            {
                PinchToZoom(args.X0, args.Y0, args.X1, args.Y1);
            }
            else if (action == InputActions.Merge)
            {
                FinishCurrentElement(args.X, args.Y);
            }
            else if (action == InputActions.StartPan)
            {
                SetPanStart(args.Index == 0 ? args.X0 : args.X1, 
                    args.Index == 0 ? args.Y0 : args.Y1);
            }
        }

        #endregion

        #region Draw Elements

        private void DrawPolygon(Canvas canvas, Paint paint, PolygonF poly)
        {
            int sides = poly.Sides;
            float x1, y1, x2, y2;

            int i, j = 0;

            for (i = 0, j = sides - 1; i < sides; j = i++)
            {
                x1 = poly.GetX(i);
                y1 = poly.GetY(i);
                x2 = poly.GetX(j);
                y2 = poly.GetY(j);

                canvas.DrawLine(x1, y1, x2, y2, paint);
            }
        }

        private void DrawWire(Canvas canvas, Wire wire)
        {
            float x1 = wire.Start.X;
            float y1 = wire.Start.Y;
            float x2 = wire.End.X;
            float y2 = wire.End.Y;

            if (wire.IsSelected == true)
                wirePaint.Color = colorSelected;
            else
                wirePaint.Color = colorElement;

            // draw element shape
            canvas.DrawLine(x1, y1, x2, y2, wirePaint);

            // draw wire bounds
            if (wire.IsSelected == true)
            {
                DrawPolygon(canvas, boundsPaint, wire.WireBounds);

                canvas.DrawRect(wire.StartBounds, boundsPaint);
                canvas.DrawRect(wire.EndBounds, boundsPaint);
            }
        }

        private void DrawPin(Canvas canvas, Pin pin)
        {
            if (pin.IsSelected == true || pin.ShowPins == true)
                pinPaint.Color = colorSelected;
            else
                pinPaint.Color = colorElement;

            canvas.DrawCircle(pin.X, pin.Y, pin.Radius, pinPaint);

            if (pin.IsSelected == true || pin.ShowPins == true)
                canvas.DrawRect(pin.Bounds, boundsPaint);
        }

        private void DrawPins(Canvas canvas, List<Pin> pins)
        {
            foreach (var pin in pins)
                DrawPin(canvas, pin);
        }

        private void SetElementColor(Element element)
        {
            if (element.IsSelected == true)
            {
                elementPaint.Color = colorSelected;
                textElementPaint.Color = colorSelected;
            }
            else
            {
                elementPaint.Color = colorElement;
                textElementPaint.Color = colorElement;
            }
        }

        private void DrawAndGate(Canvas canvas, AndGate andGate)
        {
            float x = andGate.X;
            float y = andGate.Y;
            bool showPins = andGate.ShowPins;
            float w = andGate.Width; // element width
            float h = andGate.Height; // element height

            SetElementColor(andGate);

            // draw element shape
            canvas.DrawLine(x + 0f, y + 0f, x + w, y + 0f, elementPaint);
            canvas.DrawLine(x + w, y + 0f, x + w, y + h, elementPaint);
            canvas.DrawLine(x + w, y + h, x + 0f, y + h, elementPaint);
            canvas.DrawLine(x + 0f, y + h, x + 0f, y + 0f, elementPaint);

            // draw text in center of element shape
            string text = "&";
            float textVerticalOffset = (textElementPaint.Descent() + textElementPaint.Ascent()) / 2f;
            canvas.DrawText(text, x + w / 2f, y + (h / 2f) - textVerticalOffset, textElementPaint);

            // draw element pins
            if (showPins == true)
                DrawPins(canvas, andGate.Pins);
        }

        private void DrawOrGate(Canvas canvas, OrGate orGate)
        {
            float x = orGate.X;
            float y = orGate.Y;
            bool showPins = orGate.ShowPins;
            int counter = orGate.Counter;
            float w = orGate.Width; // element width
            float h = orGate.Height; // element height

            SetElementColor(orGate);

            // draw element shape
            canvas.DrawLine(x + 0f, y + 0f, x + w, y + 0f, elementPaint);
            canvas.DrawLine(x + w, y + 0f, x + w, y + h, elementPaint);
            canvas.DrawLine(x + w, y + h, x + 0f, y + h, elementPaint);
            canvas.DrawLine(x + 0f, y + h, x + 0f, y + 0f, elementPaint);

            // draw text in center of element shape
            string text = "≥" + counter.ToString(); // default counter is 1
            float textVerticalOffset = (textElementPaint.Descent() + textElementPaint.Ascent()) / 2f;
            canvas.DrawText(text, x + w / 2f, y + (h / 2f) - textVerticalOffset, textElementPaint);

            // draw element pins
            if (showPins == true)
                DrawPins(canvas, orGate.Pins);
        }

        #endregion

        #region Draw Diagram

        private void ScaleStrokeWidth()
        {
            wirePaint.StrokeWidth = wireStrokeWidth / zoom;
            elementPaint.StrokeWidth = elementStrokeWidth / zoom;
            pinPaint.StrokeWidth = pinStrokeWidth / zoom;
            //boundsPaint.StrokeWidth = pinStrokeWidth / zoom;
            //pagePaint.StrokeWidth = pageStrokeWidth / zoom;
            gridPaint.StrokeWidth = gridStrokeWidth / zoom;
        }

        private void TransformCanvas(Canvas canvas)
        {
            // pan and zoom
            canvas.Matrix = matrix;
        }

        private void DrawBackgorud(Canvas canvas)
        {
            canvas.DrawColor(colorBackground);
        }

        private void DrawGrid(Canvas canvas, Paint paint, 
                              float ox, float oy,
                              float width, float height, 
                              float sx, float sy)
        {
            // horizontal lines
            for (float y = sy + oy; y < height + oy; y += sx)
                canvas.DrawLine(ox, y, width + ox, y, paint);

            // vertical lines
            for (float x = sx + ox; x < width + ox; x += sx)
                canvas.DrawLine(x, oy, x, height + oy, paint);

            // box rect
            float sw = paint.StrokeWidth;
            paint.StrokeWidth *= 2f;
            canvas.DrawRect(ox, oy, width + ox, height + oy, paint);
            paint.StrokeWidth = sw;
        }

        private void DrawElements(Canvas canvas)
        {
            foreach (var pair in Elements)
            {
                var element = pair.Value;

                if (element is Wire)
                    DrawWire(canvas, element as Wire);
                else if (element is AndGate)
                    DrawAndGate(canvas, element as AndGate);
                else if (element is OrGate)
                    DrawOrGate(canvas, element as OrGate);
                else if (element is Pin)
                    DrawPin(canvas, element as Pin);
            }
        }

        public void DrawWidgets(Canvas canvas)
        {
            float ox = -(translate.X / zoom);
            float oy = -(translate.Y / zoom);
            widgetBounds.Set(ox, oy, ((float)this.SurfaceWidth / zoom) + ox, ((float)this.SurfaceHeight / zoom) + oy);

            test.StrokeWidth = 4f / zoom;
            canvas.DrawRect(widgetBounds, test);

            // TODO: Draw widgets.
        }

        public void DrawDiagram(Canvas canvas)
        {
            if (canvas == null)
                return;

            canvas.Save();

            ScaleStrokeWidth();
            TransformCanvas(canvas);
            DrawBackgorud(canvas);

            // grid size and origin
            float sx = 30f;
            float sy = 30f;
            float ox = (SurfaceWidth % sx) / 2f;
            float oy = (SurfaceHeight % sy) / 2f;

            DrawGrid(canvas, gridPaint, 
                ox, oy, 
                SurfaceWidth - (ox + ox), 
                SurfaceHeight - (oy + oy), 
                sx, sy);

            DrawElements(canvas);
            //DrawWidgets (canvas);

            canvas.Restore();
        }

        #endregion

        #region Redraw

        private static Action<InputArgs, InputArgs> CopyInputArgs = (src, dst) =>
        {
            if (src != null && dst != null)
            {
                dst.Action = src.Action;
                dst.X = src.X;
                dst.Y = src.Y;
                dst.Index = src.Index;
                dst.X0 = src.X0;
                dst.Y0 = src.Y0;
                dst.X1 = src.X1;
                dst.Y1 = src.Y1;
            }
        };

        public void RedrawCanvas()
        {
            if (Service != null)
            {
                var result = Service.HandleEvent(EmptyArgs, CopyInputArgs, 16);
                //if (result == false)
                //  Console.WriteLine("Skip: " + FrameCount);

                FrameCount++;
            }
        }

        public void RedrawCanvas(InputArgs args)
        {
            if (Service != null)
            {
                var result = Service.HandleEvent(args, CopyInputArgs, 16);
                //if (result == false)
                //  Console.WriteLine("Skip: " + FrameCount);

                FrameCount++;
            }
        }

        #endregion

        #region Reset

        public void Reset()
        {
            Clear();
            RedrawCanvas();
        }

        #endregion

        #region Zoom

        public void SetZoom(float scale, float midX, float midY, float transX, float transY)
        {
            matrix.Reset();

            matrix.PostTranslate(transX, transY);
            matrix.PostScale(scale, scale, midX, midY);

            savedMatrix.Set(matrix);

            translate.Set(transX, transY);
            middle.Set(midX, midY);
            zoom = scale;
        }

        public void ResetZoom()
        {
            zoom = 1f;
            translate.Set(0f, 0f);

            start.Set(0f, 0f);
            previousDist = 0f;
            middle.Set(0f, 0f);
            matrix.Reset();
            savedMatrix.Reset();
        }

        #endregion

        #region Insert

        public void GetCenterPoint(out float x, out float y)
        {
            float ox = -(translate.X / zoom);
            float oy = -(translate.Y / zoom);
            var rect = new RectF(ox, oy, ((float)this.SurfaceWidth / zoom) + ox, ((float)this.SurfaceHeight / zoom) + oy);

            x = rect.CenterX();
            y = rect.CenterY();
        }

        public Pin InsertPin(float x, float y, bool redraw)
        {
            int id = GetNextId();
            var pin = new Pin(id, null, x, y, 4f, 3f);

            Elements.TryAdd(id, pin);

            if (redraw == true)
                RedrawCanvas();

            return pin;
        }

        public Wire InsertWire(Pin startPin, Pin endPin, bool redraw)
        {
            int id = GetNextId();
            var wire = new Wire(id, startPin, endPin, 4f, 3f);

            Elements.TryAdd(id, wire);

            // update pin to wire connections
            startPin.Wires.Add(wire);
            endPin.Wires.Add(wire);

            // redraw if requested
            if (redraw == true)
                RedrawCanvas();

            return wire;
        }

        public AndGate InsertAndGate(float x, float y, bool redraw)
        {
            int id = GetNextId();
            var andGate = new AndGate(id, x, y);

            Elements.TryAdd(id, andGate);

            if (redraw == true)
                RedrawCanvas();

            return andGate;
        }

        public OrGate InsertOrGate(float x, float y, bool redraw)
        {
            int id = GetNextId();
            var orGate = new OrGate(id, x, y, 1);

            Elements.TryAdd(id, orGate);

            if (redraw == true)
                RedrawCanvas();

            return orGate;
        }

        #endregion

        #region Model

        public void Snapshot()
        {
            string model = Editor.Generate(this.Elements);
            undoStack.Push(model);

            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count <= 0)
                return;

            string model;
            if (undoStack.TryPop(out model))
            {
                string current = Editor.Generate(this.Elements);
                redoStack.Push(current);

                Clear();
                Editor.Parse(model, this.Elements);
                UpdateNextId();

                RedrawCanvas();
            }
        }

        public void Redo()
        {
            if (redoStack.Count <= 0)
                return;

            string model;
            if (redoStack.TryPop(out model))
            {
                string current = Editor.Generate(this.Elements);
                undoStack.Push(current);

                Clear();
                Editor.Parse(model, this.Elements);
                UpdateNextId();

                RedrawCanvas();
            }
        }

        public void Clear()
        {
            Elements.Clear();
            SetNextId(0);
        }

        #endregion
    }

    #endregion

    #region UI

    [Activity(Label = "Diagram Editor")]
    public class DiagramEditor : Activity
    {
        #region Fields

        private DrawingView drawingView;
        private DiagramRepository repository;
        private Diagram currentDiagram;

        #endregion

        #region Activity Lifecycle

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            repository = new DiagramRepository();

            // create drawing canvas
            drawingView = new DrawingView(this);

            RegisterForContextMenu(drawingView);

            // get diagram from repository
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
                currentDiagram = repository.Get(diagramId);
            else
                currentDiagram = new Diagram();

            // create diagram
            Editor.Parse(currentDiagram.Model, drawingView.Service.Elements);
            drawingView.Service.UpdateNextId();
            drawingView.Service.CurrentModel = Editor.Generate(drawingView.Service.Elements);

            // set content view to drawing canvas
            SetContentView(drawingView);

            //ActivityCompat.InvalidateOptionsMenu (this);

            //Console.WriteLine ("DiagramEditor OnCreate");
        }

        protected override void OnStop()
        {
            base.OnStop();

            // store diagram model
            currentDiagram.Model = Editor.Generate(drawingView.Service.Elements);
            currentDiagram.Id = repository.Save(currentDiagram);

            //Console.WriteLine ("DiagramEditor OnStop");
        }

        protected override void OnPause()
        {
            base.OnPause();

            //Console.WriteLine ("DiagramEditor OnPause");

            // stop drawing thread
            drawingView.Service.Stop();
        }

        protected override void OnResume()
        {
            base.OnResume();

            //Console.WriteLine ("DiagramEditor OnResume");

            // start drawing thread
            drawingView.Service.Start(drawingView.Holder);
        }

        #endregion

        #region Options Menu

        private const int ItemGroupInsert = 0;
        private const int ItemGroupEdit = 1;

        private const int ItemInsertAndGate = 0;
        private const int ItemInsertOrGate = 1;

        private const int ItemResetDiagram = 2;
        private const int ItemResetZoom = 3;
        private const int ItemEditUndo = 4;
        private const int ItemEditRedo = 5;

        public bool HabdleItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case ItemInsertAndGate:
                    {
                        float x, y;
                        drawingView.Service.Snapshot();
                        drawingView.Service.GetCenterPoint(out x, out y);
                        drawingView.Service.InsertAndGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemInsertOrGate:
                    {
                        float x, y;
                        drawingView.Service.Snapshot();
                        drawingView.Service.GetCenterPoint(out x, out y);
                        drawingView.Service.InsertOrGate(x - 15f, y - 15f, true);
                    }
                    return true;
                case ItemResetDiagram:
                    drawingView.Service.Snapshot();
                    drawingView.Service.Reset();
                    return true;
                case ItemResetZoom:
                    drawingView.Service.ResetZoom();
                    drawingView.Service.RedrawCanvas();
                    return true;
                case ItemEditUndo:
                    drawingView.Service.Undo();
                    return true;
                case ItemEditRedo:
                    drawingView.Service.Redo();
                    return true;
                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public static void CreateOptionsMenu(IMenu menu)
        {
            // insert
            menu.Add(ItemGroupInsert, ItemInsertAndGate, 0, "Insert AndGate");
            menu.Add(ItemGroupInsert, ItemInsertOrGate, 1, "Insert OrGate");
            // edit
            menu.Add(ItemGroupEdit, ItemResetDiagram, 2, "Reset Diagram");
            menu.Add(ItemGroupEdit, ItemResetZoom, 3, "Reset Zoom");
            menu.Add(ItemGroupEdit, ItemEditUndo, 4, "Undo");
            menu.Add(ItemGroupEdit, ItemEditRedo, 5, "Redo");
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            CreateOptionsMenu(menu);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            return HabdleItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            CreateOptionsMenu(menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return HabdleItemSelected(item);
        }

        #endregion

        #region Instance State

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
        }

        #endregion
    }

    [Activity(Label = "Canvas Diagram", MainLauncher = true)]          
    public class DiagramList : Activity
    {
        #region Fields

        private ListView listViewDiagrams;

        private DiagramRepository repository;
        private List<Diagram> diagrams;

        #endregion

        #region Activity Lifecycle

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.DiagramList);

            /*

            //
            // Begin: new Core.Test
            //

            try
            {
                var sb = new StringBuilder ();
                var nl = CanvasDiagram.Core.Test.Constants.NewLine;

                sb.Append ("PS;0;255;255;0;0;1;1.0;4.0"); sb.Append (nl);
                sb.Append ("LS;1;255;255;0;0;0;1.0"); sb.Append (nl);
                sb.Append ("RS;2;255;255;0;0;0;1.0"); sb.Append (nl);
                sb.Append ("CS;3;255;255;0;0;0;1.0"); sb.Append (nl);
                sb.Append ("AS;4;255;255;0;0;0;1.0"); sb.Append (nl);
                sb.Append ("TS;5;255;255;0;0;0;1.0;1;1;14.0"); sb.Append (nl);

                var data = sb.ToString ();
                var factory = new CanvasDiagram.Droid.Renderers.RendererFactory ("Renderer", "CanvasDiagram.Droid.Renderers");
                var model = CanvasDiagram.Core.Test.Editor.Parse (data, factory);
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Parse Error: {0}", ex.Message);
                Console.WriteLine (ex.StackTrace);
            }

            //
            // End: new Core.Test
            //

            */

            repository = new DiagramRepository();

            // connect view elements
            listViewDiagrams = FindViewById<ListView>(Resource.Id.listViewDiagrams);

            // edit diagram model
            listViewDiagrams.ItemClick += (sender, e) =>
            {
                var diagramEidtor = new Intent(this, typeof(DiagramEditor));
                diagramEidtor.PutExtra("DiagramId", diagrams[e.Position].Id);

                StartActivity(diagramEidtor);
            };

            // edit diagram properties
            listViewDiagrams.ItemLongClick += (sender, e) =>
            {
                var diagramProperties = new Intent(this, typeof(DiagramProperties));
                diagramProperties.PutExtra("DiagramId", diagrams[e.Position].Id);

                StartActivity(diagramProperties);
            };

            Console.WriteLine("DiagramList OnResume");
        }

        protected override void OnResume()
        {
            base.OnResume();

            // get diagrams from repository
            diagrams = repository.GetAll();

            // set diagram list adapter
            var adapter = new DiagramListAdapter(this, diagrams);
            listViewDiagrams.Adapter = adapter;

            Console.WriteLine("DiagramList OnResume");
        }

        #endregion

        #region Repository

        public void DeleteAll()
        {
            repository.RemoveAll();
            diagrams.Clear();
            (listViewDiagrams.Adapter as DiagramListAdapter).NotifyDataSetChanged();
        }

        #endregion

        #region Options Menu

        private const int ItemGroupAdd = 0;

        private const int ItemAddDiagram = 0;
        private const int ItemDeleteAll = 1;

        public bool HabdleItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case ItemAddDiagram:
                    StartActivity(typeof(DiagramProperties));
                    return true;
                case ItemDeleteAll:
                    {
                        AlertDialog.Builder ab = new AlertDialog.Builder(this);
                        ab.SetMessage("Delete All diagrams?")
                            .SetNegativeButton("No", (sender, e) =>
                            {
                            })
                            .SetPositiveButton("Yes", (sender, e) => DeleteAll())
                            .Show();
                    }
                    return true;
                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public static void CreateOptionsMenu(IMenu menu)
        {
            menu.Add(ItemGroupAdd, ItemAddDiagram, 0, "Add Diagram");
            menu.Add(ItemGroupAdd, ItemDeleteAll, 1, "Delete All");
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            CreateOptionsMenu(menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return HabdleItemSelected(item);
        }

        #endregion
    }

    public class DiagramListAdapter : BaseAdapter<Diagram>
    {
        protected Activity context = null;
        protected IList<Diagram> diagrams = new List<Diagram>();

        public DiagramListAdapter(Activity context, IList<Diagram> diagrams)
            : base()
        {
            this.context = context;
            this.diagrams = diagrams;
        }

        public override Diagram this [int index]
        {
            get { return diagrams[index]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return diagrams.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var diagram = diagrams[position];

            var view = (convertView ??
                       context.LayoutInflater.Inflate(
                           Android.Resource.Layout.SimpleListItem1,
                           parent, 
                           false)) as TextView;

            view.SetText(diagram.Title, TextView.BufferType.Normal);

            return view;
        }
    }

    [Activity(Label = "Diagram Properties")]           
    public class DiagramProperties : Activity
    {
        #region Fields

        private Button buttonCancel;
        private Button buttonDelete;
        private Button buttonSave;

        private EditText editTextTitle;
        private EditText editTextModel;

        private DiagramRepository repository;
        private Diagram currentDiagram;

        #endregion

        #region Activity Lifecycle

        protected override void OnCreate(Bundle bundle)
        {
            //RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.DiagramProperties);

            repository = new DiagramRepository();

            // get diagram from repository
            bool isExistingDiagram = false;
            int diagramId = Intent.GetIntExtra("DiagramId", 0);
            if (diagramId > 0)
            {
                currentDiagram = repository.Get(diagramId);
                isExistingDiagram = true;
            }
            else
            {
                // create new diagram
                currentDiagram = new Diagram();
            }

            // connect view elements
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonDelete = FindViewById<Button>(Resource.Id.buttonDelete);
            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            editTextTitle = FindViewById<EditText>(Resource.Id.editTextTitle);
            editTextModel = FindViewById<EditText>(Resource.Id.editTextModel);

            // enable delete button for existing diagrams
            buttonDelete.Enabled = isExistingDiagram;

            // cancel
            buttonCancel.Click += (sender, e) =>
            {
                Finish();
            };

            // delete
            buttonDelete.Click += (sender, e) =>
            {
                repository.Delete(currentDiagram);

                Finish();
            };

            // save
            buttonSave.Click += (sender, e) =>
            {
                currentDiagram.Title = editTextTitle.Text;
                currentDiagram.Model = editTextModel.Text;
                currentDiagram.Id = repository.Save(currentDiagram);

                Finish();
            };

            // set current diagram properties
            editTextTitle.Text = currentDiagram.Title;
            editTextModel.Text = currentDiagram.Model;

            Console.WriteLine("DiagramProperties OnCreate");
        }

        protected override void OnResume()
        {
            base.OnResume();

            Console.WriteLine("DiagramProperties OnResume");
        }

        #endregion
    }

    #endregion
}
