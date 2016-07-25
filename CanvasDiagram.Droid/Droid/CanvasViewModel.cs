using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Android.Graphics;
using Android.Views;

namespace CanvasDiagram.Droid
{
    public class CanvasViewModel
    {
        public ConcurrentDictionary<int, Element> Elements { get; set; }
        public string CurrentModel { get; set; }
        public int SurfaceWidth { get; set; }
        public int SurfaceHeight { get; set; }

        private SurfaceViewThread<InputArgs> Service;
        private int FrameCount = 0;
        private InputArgs EmptyArgs = new InputArgs() { Action = InputAction.None };

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

        public void Start(ISurfaceHolder surfaceHolder)
        {
            try
            {
                if (Service == null)
                {
                    Service = new SurfaceViewThread<InputArgs>();
                }

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

        public void PinchToZoom(float x0, float y0, float x1, float y1)
        {
            float currentDist = LineUtil.Distance(x0, y0, x1, y1);

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
            previousDist = LineUtil.Distance(x0, y0, x1, y1);

            if (previousDist > minPinchToZoomDistance)
            {
                savedMatrix.Set(matrix);

                LineUtil.Middle(ref middle, x0, y0, x1, y1);
            }
        }

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
            var insert = findClosest ? VectorUtil.NearestPointOnLine(a, b, p) : p;

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

        public void HandleInput(InputArgs args)
        {
            var action = args.Action;

            if (action == InputAction.Hitest)
            {
                HandleOneInputDown(args.X, args.Y);
            }
            else if (action == InputAction.Move)
            {
                HandleOneInputMove(args.X, args.Y);
            }
            else if (action == InputAction.StartZoom)
            {
                StartPinchToZoom(args.X0, args.Y0, args.X1, args.Y1);
            }
            else if (action == InputAction.Zoom)
            {
                PinchToZoom(args.X0, args.Y0, args.X1, args.Y1);
            }
            else if (action == InputAction.Merge)
            {
                FinishCurrentElement(args.X, args.Y);
            }
            else if (action == InputAction.StartPan)
            {
                SetPanStart(args.Index == 0 ? args.X0 : args.X1, 
                    args.Index == 0 ? args.Y0 : args.Y1);
            }
        }

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

        private void DrawGrid(Canvas canvas, Paint paint, float ox, float oy, float width, float height, float sx, float sy)
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

        public void Reset()
        {
            Clear();
            RedrawCanvas();
        }

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

        public void Snapshot()
        {
            string model = TextSerializer.Serialize(this.Elements);
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
                string current = TextSerializer.Serialize(this.Elements);
                redoStack.Push(current);

                Clear();
                TextSerializer.Deserialize(model, this.Elements);
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
                string current = TextSerializer.Serialize(this.Elements);
                undoStack.Push(current);

                Clear();
                TextSerializer.Deserialize(model, this.Elements);
                UpdateNextId();

                RedrawCanvas();
            }
        }

        public void Clear()
        {
            Elements.Clear();
            SetNextId(0);
        }
    }
}
