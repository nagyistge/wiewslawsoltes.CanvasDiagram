// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#define CONNECTORS

using MathUtil;
using RxCanvas.Interfaces;
using RxCanvas.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxCanvas.Editors
{
    internal static class Helper
    {
        public static SeparatingAxisTheorem SAT = new SeparatingAxisTheorem();

        public static IPoint ConnectAt(
            IList<INative> children,
            double x,
            double y)
        {
            return children
                .Where(c => c.Bounds != null)
                .Select(c => c.Bounds.ConnectAt(x, y))
                .FirstOrDefault(p => p != null);
        }

        public static INative HitTest(
            IList<INative> children,
            double x,
            double y)
        {
            return children
                .Where(c => c.Bounds != null && c.Bounds.Contains(x, y))
                .FirstOrDefault();
        }

        public static IList<INative> HitTest(
            IList<INative> children,
            IRectangle rectangle)
        {
            double x = Math.Min(rectangle.Point1.X, rectangle.Point2.X);
            double y = Math.Min(rectangle.Point1.Y, rectangle.Point2.Y);
            double width = Math.Abs(rectangle.Point2.X - rectangle.Point1.X);
            double height = Math.Abs(rectangle.Point2.Y - rectangle.Point1.Y);

            var selectionVertices = new Vector2[]
            {
                new Vector2(x, y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y + height)
            };

            var overlapping = children
                .Where(c =>
                {
                    if (c.Bounds != null)
                    {
                        var vertices = c.Bounds.GetVertices();
                        if (vertices != null)
                        {
                            return SAT.Overlap(selectionVertices, vertices);
                        }
                        return false;
                    }
                    return false;
                }).ToList();

            foreach (var child in overlapping)
            {
                child.Bounds.Show();
            }

            return overlapping;
        }
    }

    public class XSingleEditor : IEditor, IDisposable
    {
        [Flags]
        public enum State
        {
            None = 0,
            Hover = 1,
            Selected = 2,
            Move = 4,
            HoverSelected = Hover | Selected,
            HoverMove = Hover | Move,
            SelectedMove = Selected | Move
        }

        public string Name { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled)
                {
                    Reset();
                }
                _isEnabled = value;
            }
        }

        public string Key { get; set; }
        public string Modifiers { get; set; }

        private INativeConverter _nativeConverter;
        private ICanvasFactory _canvasFactory;
        private IBoundsFactory _boundsFactory;
        private ICanvas _canvas;
        private Vector2 _original;
        private Vector2 _start;
        private INative _selected;
        private INative _hover;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _ups;
        private IDisposable _drag;

        public XSingleEditor(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _nativeConverter = nativeConverter;
            _canvasFactory = canvasFactory;
            _boundsFactory = boundsFactory;

            _canvas = canvas;

            Name = "Single Selection";
            Key = "H";
            Modifiers = "";

            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, _canvas.Moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p => Down(p));
            _ups = _canvas.Ups.Where(_ => IsEnabled).Subscribe(p => Up(p));
            _drag = drags.Where(_ => IsEnabled).Subscribe(p => Drag(p));
        }

        private bool IsState(State state)
        {
            return (_state & state) == state;
        }

        private void Down(Vector2 p)
        {
            bool render = false;

            if (IsState(State.Selected))
            {
                HideSelected();
                render = true;
            }

            if (IsState(State.Hover))
            {
                HideHover();
                render = true;
            }

            _selected = Helper.HitTest(_canvas.Children, p.X, p.Y);
            if (_selected != null)
            {
                ShowSelected();
                InitMove(p);
                _canvas.Capture();
                render = true;
            }

            if (render)
            {
                _canvas.Render(null);
            }
        }

        private void Up(Vector2 p)
        {
            if (_canvas.IsCaptured)
            {
                if (IsState(State.Move))
                {
                    FinishMove(p);
                    _canvas.ReleaseCapture();
                }
            }
        }

        private void Drag(Vector2 p)
        {
            if (_canvas.IsCaptured)
            {
                if (IsState(State.Move))
                {
                    Move(p);
                }
            }
            else
            {
                bool render = false;
                var result = Helper.HitTest(_canvas.Children, p.X, p.Y);

                if (IsState(State.Hover))
                {
                    if (IsState(State.Selected))
                    {
                        if (_hover != _selected && _hover != result)
                        {
                            HideHover();
                            render = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (result != _hover)
                        {
                            HideHover();
                            render = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (result != null)
                {
                    if (IsState(State.Selected))
                    {
                        if (result != _selected)
                        {
                            _hover = result;
                            ShowHover();
                            render = true;
                        }
                    }
                    else
                    {
                        _hover = result;
                        ShowHover();
                        render = true;
                    }
                }

                if (render)
                {
                    _canvas.Render(null);
                }
            }
        }

        private void ShowHover()
        {
            _hover.Bounds.Show();
            _state |= State.Hover;
            Debug.Print("_state: {0}", _state);
        }

        private void HideHover()
        {
            _hover.Bounds.Hide();
            _hover = null;
            _state = _state & ~State.Hover;
            Debug.Print("_state: {0}", _state);
        }

        private void ShowSelected()
        {
            _selected.Bounds.Show();
            _state |= State.Selected;
            Debug.Print("_state: {0}", _state);
        }

        private void HideSelected()
        {
            _selected.Bounds.Hide();
            _selected = null;
            _state = _state & ~State.Selected;
            Debug.Print("_state: {0}", _state);
        }

        private void InitMove(Vector2 p)
        {
            // TODO: Create history snapshot but do not push undo.
            _original = p;
            _start = p;
            _state |= State.Move;
            Debug.Print("_state: {0}", _state);
        }

        private void FinishMove(Vector2 p)
        {
            if (p.X == _original.X && p.Y == _original.Y)
            {
                // TODO: Do not push history undo.
            }
            else
            {
                // TODO: Push history undo.
            }
            _state = _state & ~State.Move;
            Debug.Print("_state: {0}", _state);
        }

        private void Move(Vector2 p)
        {
            if (_selected != null)
            {
                double dx = _start.X - p.X;
                double dy = _start.Y - p.Y;
                _start = p;
                _selected.Bounds.MoveContaining(dx, dy);
                _selected.Bounds.Update();
                _canvas.Render(null);
            }
        }

        private void Reset()
        {
            bool render = false;

            if (_hover != null)
            {
                _hover.Bounds.Hide();
                _hover = null;
                render = true;
            }

            if (_selected != null)
            {
                _selected.Bounds.Hide();
                _selected = null;
                render = true;
            }

            _state = State.None;
            Debug.Print("_state: {0}", _state);

            if (render)
            {
                _canvas.Render(null);
            }
        }

        public void Dispose()
        {
            _downs.Dispose();
            _ups.Dispose();
            _drag.Dispose();
        }
    }

    public class XMultiEditor : IEditor, IDisposable
    {
        [Flags]
        public enum State 
        { 
            None = 0,
            Selection = 1,
            Overlapping = 2,
            Move = 4
        }

        public string Name { get; set; }

        private bool _isEnabled;
        public bool IsEnabled 
        {
            get { return _isEnabled; }
            set 
            {
                if (_isEnabled)
                {
                    _canvas.EnableSnap = _enableSnapOriginal;

                    Reset();
                }
                _isEnabled = value; 

                if (_isEnabled)
                {
                    _enableSnapOriginal = _canvas.EnableSnap;
                    _canvas.EnableSnap = false;
                }
            }
        }

        public string Key { get; set; }
        public string Modifiers { get; set; }

        private INativeConverter _nativeConverter; 
        private ICanvasFactory _canvasFactory;
        private IBoundsFactory _boundsFactory;
        private ICanvas _canvas;
        private bool _enableSnapOriginal;
        private Vector2 _original;
        private Vector2 _start;
        private IList<INative> _overlapping;
        private IRectangle _xrectangle;
        private IRectangle _nrectangle;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _ups;
        private IDisposable _drag;

        public XMultiEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory, 
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _nativeConverter = nativeConverter;
            _canvasFactory = canvasFactory;
            _boundsFactory = boundsFactory;

            _canvas = canvas;

            Name = "Multi Selection";
            Key = "J";
            Modifiers = "";

            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, _canvas.Moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p => Down(p));
            _ups = _canvas.Ups.Where(_ => IsEnabled).Subscribe(p => Up(p));
            _drag = drags.Where(_ => IsEnabled).Subscribe(p => Drag(p));
        }

        private void Down(Vector2 p)
        {
            switch(_state)
            {
                case State.None:
                    {
                        InitSelection(p);
                        _canvas.Render(null);
                        _state = State.Selection;
                        Debug.Print("_state: {0}", _state);
                    }
                    break;
                case State.Selection:
                    {
                        ResetSelection();
                        _canvas.Render(null);
                        _state = State.None;
                        Debug.Print("_state: {0}", _state);
                    }
                    break;
                case State.Overlapping:
                    {
                        if (Helper.HitTest(_overlapping, p.X, p.Y) != null)
                        {
                            InitMove(p);
                            _state = State.Move;
                            Debug.Print("_state: {0}", _state);
                        }
                        else
                        {
                            ResetOverlaping();
                            //_canvas.Render(null);
                            //_state = State.None;
                            //Debug.Print("_state: {0}", _state);

                            InitSelection(p);
                            _canvas.Render(null);
                            _state = State.Selection;
                            Debug.Print("_state: {0}", _state);
                        }
                    }
                    break;
                case State.Move:
                    {
                        ResetOverlaping();
                        _canvas.Render(null);
                        _state = State.None;
                        Debug.Print("_state: {0}", _state);
                    }
                    break;
            };
        }

        private void Up(Vector2 p)
        {
            switch (_state)
            {
                case State.None:
                    {

                    }
                    break;
                case State.Selection:
                    {
                        FinishSelection();
                        _canvas.Render(null);
                    }
                    break;
                case State.Overlapping:
                    {
                        
                    }
                    break;
                case State.Move:
                    {
                        FinishMove(p);
                        _state = State.Overlapping;
                        Debug.Print("_state: {0}", _state);
                    }
                    break;
            };
        }

        private void Drag(Vector2 p)
        {
            switch (_state)
            {
                case State.None:
                    {

                    }
                    break;
                case State.Selection:
                    {
                        MoveSelection(p);
                        _canvas.Render(null);
                    }
                    break;
                case State.Overlapping:
                    {

                    }
                    break;
                case State.Move:
                    {
                        Move(p);
                        _canvas.Render(null);
                    }
                    break;
            };
        }

        private void InitSelection(Vector2 p)
        {
            if (_xrectangle == null)
            {
                _xrectangle = _canvasFactory.CreateRectangle();
                _xrectangle.StrokeThickness = 2.0;
                _xrectangle.Stroke.A = 0xFF;
                _xrectangle.Stroke.R = 0x33;
                _xrectangle.Stroke.G = 0x99;
                _xrectangle.Stroke.B = 0xFF;
                _xrectangle.Fill.A = 0x3F;
                _xrectangle.Fill.R = 0x33;
                _xrectangle.Fill.G = 0x99;
                _xrectangle.Fill.B = 0xFF;
            }

            _xrectangle.Point1.X = p.X;
            _xrectangle.Point1.Y = p.Y;
            _xrectangle.Point2.X = p.X;
            _xrectangle.Point2.Y = p.Y;

            if (_nrectangle == null)
            {
                _nrectangle = _nativeConverter.Convert(_xrectangle);
            }

            _canvas.Add(_nrectangle);

            if (_nrectangle.Bounds == null)
            {
                _nrectangle.Bounds = _boundsFactory.Create(_canvas, _nrectangle);
            }

            _nrectangle.Bounds.Update();
        }

        private void MoveSelection(Vector2 p)
        {
            _xrectangle.Point2.X = p.X;
            _xrectangle.Point2.Y = p.Y;
            _nrectangle.Point2 = _xrectangle.Point2;
            _nrectangle.Bounds.Update();
        }

        private void FinishSelection()
        {
            ResetSelection();

            _overlapping = Helper.HitTest(_canvas.Children, _xrectangle);
            if (_overlapping.Count > 0)
            {
                _state = State.Overlapping;
            }
            else
            {
                _state = State.None;
            }

            Debug.Print("_state: {0}", _state);
        }

        private void ResetOverlaping()
        {
            foreach (var child in _overlapping)
            {
                child.Bounds.Hide();
            }
            _overlapping = null;
        }

        private void ResetSelection()
        {
            //_xrectangle.Point2.X = p.X;
            //_xrectangle.Point2.Y = p.Y;
            //_nrectangle.Point2 = _xrectangle.Point2;
            _nrectangle.Bounds.Update();
            _canvas.Remove(_nrectangle);
            //_canvas.Render(null);
        }

        private void InitMove(Vector2 p)
        {
            // TODO: Create history snapshot but do not push undo.
            double x = _enableSnapOriginal ? _canvas.Snap(p.X, _canvas.SnapX) : p.X;
            double y = _enableSnapOriginal ? _canvas.Snap(p.Y, _canvas.SnapY) : p.Y;
            _original = new Vector2(x, y);
            _start = new Vector2(x, y);
        }

        private void FinishMove(Vector2 p)
        {
            double x = _enableSnapOriginal ? _canvas.Snap(p.X, _canvas.SnapX) : p.X;
            double y = _enableSnapOriginal ? _canvas.Snap(p.Y, _canvas.SnapY) : p.Y;
            if (x == _original.X && y == _original.Y)
            {
                // TODO: Do not push history undo.
            }
            else
            {
                // TODO: Push history undo.
            }
        }

        private void Move(Vector2 p)
        {
            if (_overlapping != null)
            {
                double x = _enableSnapOriginal ? _canvas.Snap(p.X, _canvas.SnapX) : p.X;
                double y = _enableSnapOriginal ? _canvas.Snap(p.Y, _canvas.SnapY) : p.Y;
                double dx = _start.X - x;
                double dy = _start.Y - y;
                _start = new Vector2(x, y);
                Debug.Print("move: {0}", _overlapping.Count);
                foreach (var child in _overlapping)
                {
                    child.Bounds.MoveAll(dx, dy);
                    child.Bounds.Update();
                }
            }
        }

        private void Reset()
        {
            switch (_state)
            {
                case State.None:
                    {

                    }
                    break;
                case State.Selection:
                    {
                        ResetSelection();
                        _canvas.Render(null);
                    }
                    break;
                case State.Overlapping:
                    {
                        ResetOverlaping();
                        _canvas.Render(null);
                    }
                    break;
                case State.Move:
                    {
                        ResetOverlaping();
                        _canvas.Render(null);
                    }
                    break;
            };

            _state = State.None;
            Debug.Print("_state: {0}", _state);
        }

        public void Dispose()
        {
            _downs.Dispose();
            _ups.Dispose();
            _drag.Dispose();
        }
    }

    public class XPinEditor : IEditor, IDisposable
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IPin _xpin;
        private IPin _npin;
        private IDisposable _downs;

        public XPinEditor(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Pin";
            Key = "P";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                _xpin = canvasFactory.CreatePin();
                _xpin.Point.X = p.X;
                _xpin.Point.Y = p.Y;
                _npin = nativeConverter.Convert(_xpin);
                _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                ConnectPoint(p);
#endif
                _canvas.Add(_npin);
                _npin.Bounds = boundsFactory.Create(_canvas, _npin);
                _npin.Bounds.Update();
                _canvas.Render(null);
            });
        }

#if CONNECTORS

        private void ConnectPoint(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xpin)
            {
                _npin.Point = connector;
                connector.Connected.Add(_npin);
            }
        } 

#endif

        public void Dispose()
        {
            _downs.Dispose();
        }
    }

    public class XLineEditor : IEditor, IDisposable
    {
        public enum State { None, Start, End }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private ILine _xline;
        private ILine _nline;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XLineEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Line";
            Key = "L";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    //_xline.Point2.X = p.X;
                    //_xline.Point2.Y = p.Y;
                    //_nline.Point2 = _xline.Point2;
#if CONNECTORS
                    ConnectPoint2(p);
#endif
                    _nline.Bounds.Hide();
                    _canvas.Render(null);
                    _state = State.None;
                    _canvas.ReleaseCapture();
                }
                else
                {
                    _xline = canvasFactory.CreateLine();
                    _xline.Point1.X = p.X;
                    _xline.Point1.Y = p.Y;
                    _xline.Point2.X = p.X;
                    _xline.Point2.Y = p.Y;
                    _nline = nativeConverter.Convert(_xline);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectPoint1(p);
#endif
                    _canvas.Add(_nline);
                    _nline.Bounds = boundsFactory.Create(_canvas, _nline);
                    _nline.Bounds.Update();
                    _nline.Bounds.Show();
                    _canvas.Capture();
                    _canvas.Render(null);
                    _state = State.End;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_state == State.End)
                {
                    _xline.Point2.X = p.X;
                    _xline.Point2.Y = p.Y;
                    _nline.Point2 = _xline.Point2;
                    _nline.Bounds.Update();
                    _canvas.Render(null);
                }
            });
        }

#if CONNECTORS

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xline)
            {
                _nline.Point1 = connector;
                connector.Connected.Add(_nline);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xline)
            {
                _nline.Point2 = connector;
                connector.Connected.Add(_nline);
            }
        } 

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XBezierEditor : IEditor, IDisposable
    {
        public enum State { None, Start, Point1, Point2, Point3 }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IBezier _xb;
        private IBezier _nb;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XBezierEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Bézier";
            Key = "B";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    switch (_state)
                    {
                        case State.Start:
                            {
                                //_xb.Point3.X = p.X;
                                //_xb.Point3.Y = p.Y;
                                //_nb.Point3 = _xb.Point3;
                                //_xb.Point2.X = p.X;
                                //_xb.Point2.Y = p.Y;
                                //_nb.Point2 = _xb.Point2;
#if CONNECTORS
                                ConnectPoint3(p);
#endif
                                _nb.Bounds.Update();
                                _canvas.Render(null);
                                _state = State.Point1;
                            }
                            break;
                        case State.Point1:
                            {
                                //_xb.Point1.X = p.X;
                                //_xb.Point1.Y = p.Y;
                                //_nb.Point1 = _xb.Point1;
#if CONNECTORS
                                ConnectPoint1(p);
#endif
                                _nb.Bounds.Update();
                                _canvas.Render(null);
                                _state = State.Point2;
                            }
                            break;
                        case State.Point2:
                            {
                                //_xb.Point2.X = p.X;
                                //_xb.Point2.Y = p.Y;
                                //_nb.Point2 = _xb.Point2;
#if CONNECTORS
                                ConnectPoint2(p);
#endif
                                _nb.Bounds.Hide();
                                _canvas.Render(null);
                                _state = State.None;
                                _canvas.ReleaseCapture();
                            }
                            break;
                    }
                }
                else
                {
                    _xb = canvasFactory.CreateBezier();
                    _xb.Start.X = p.X;
                    _xb.Start.Y = p.Y;
                    _xb.Point1.X = p.X;
                    _xb.Point1.Y = p.Y;
                    _xb.Point2.X = p.X;
                    _xb.Point2.Y = p.Y;
                    _xb.Point3.X = p.X;
                    _xb.Point3.Y = p.Y;
                    _nb = nativeConverter.Convert(_xb);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectStart(p);
#endif
                    _canvas.Add(_nb);
                    _nb.Bounds = boundsFactory.Create(_canvas, _nb);
                    _nb.Bounds.Update();
                    _nb.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.Start;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                switch (_state)
                {
                    case State.Start:
                        {
                            _xb.Point3.X = p.X;
                            _xb.Point3.Y = p.Y;
                            _nb.Point3 = _xb.Point3;
                            _xb.Point2.X = p.X;
                            _xb.Point2.Y = p.Y;
                            _nb.Point2 = _xb.Point2;
                            _nb.Bounds.Update();
                            _canvas.Render(null);
                        }
                        break;
                    case State.Point1:
                        {
                            _xb.Point1.X = p.X;
                            _xb.Point1.Y = p.Y;
                            _nb.Point1 = _xb.Point1;
                            _nb.Bounds.Update();
                            _canvas.Render(null);
                        }
                        break;
                    case State.Point2:
                        {
                            _xb.Point2.X = p.X;
                            _xb.Point2.Y = p.Y;
                            _nb.Point2 = _xb.Point2;
                            _nb.Bounds.Update();
                            _canvas.Render(null);
                        }
                        break;
                }
            });
        }

#if CONNECTORS

        private void ConnectStart(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xb)
            {
                _nb.Start = connector;
                connector.Connected.Add(_nb);
            }
        }

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xb)
            {
                _nb.Point1 = connector;
                connector.Connected.Add(_nb);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xb)
            {
                _nb.Point2 = connector;
                connector.Connected.Add(_nb);
            }
        }

        private void ConnectPoint3(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xb)
            {
                _nb.Point3 = connector;
                connector.Connected.Add(_nb);
            }
        }

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XQuadraticBezierEditor : IEditor, IDisposable
    {
        public enum State { None, Start, Point1, Point2 }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IQuadraticBezier _xqb;
        private IQuadraticBezier _nqb;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XQuadraticBezierEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Quadratic Bézier";
            Key = "Q";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    switch (_state)
                    {
                        case State.Start:
                            {
                                //_xqb.Point2.X = p.X;
                                //_xqb.Point2.Y = p.Y;
                                //_nqb.Point2 = _xqb.Point2;
#if CONNECTORS
                                ConnectPoint2(p);
#endif
                                _nqb.Bounds.Update();
                                _canvas.Render(null);
                                _state = State.Point1;
                            }
                            break;
                        case State.Point1:
                            {
                                //_xqb.Point1.X = p.X;
                                //_xqb.Point1.Y = p.Y;
                                //_nqb.Point1 = _xqb.Point1;
#if CONNECTORS
                                ConnectPoint1(p);
#endif
                                _nqb.Bounds.Hide();
                                _canvas.Render(null);
                                _state = State.None;
                                _canvas.ReleaseCapture();
                            }
                            break;
                    }
                }
                else
                {
                    _xqb = canvasFactory.CreateQuadraticBezier();
                    _xqb.Start.X = p.X;
                    _xqb.Start.Y = p.Y;
                    _xqb.Point1.X = p.X;
                    _xqb.Point1.Y = p.Y;
                    _xqb.Point2.X = p.X;
                    _xqb.Point2.Y = p.Y;
                    _nqb = nativeConverter.Convert(_xqb);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectStart(p);
#endif
                    _canvas.Add(_nqb);
                    _nqb.Bounds = boundsFactory.Create(_canvas, _nqb);
                    _nqb.Bounds.Update();
                    _nqb.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.Start;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                switch (_state)
                {
                    case State.Start:
                        {
                            _xqb.Point2.X = p.X;
                            _xqb.Point2.Y = p.Y;
                            _nqb.Point2 = _xqb.Point2;
                            _nqb.Bounds.Update();
                            _canvas.Render(null);
                        }
                        break;
                    case State.Point1:
                        {
                            _xqb.Point1.X = p.X;
                            _xqb.Point1.Y = p.Y;
                            _nqb.Point1 = _xqb.Point1;
                            _nqb.Bounds.Update();
                            _canvas.Render(null);
                        }
                        break;
                }
            });
        }

#if CONNECTORS

        private void ConnectStart(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xqb)
            {
                _nqb.Start = connector;
                connector.Connected.Add(_nqb);
            }
        }

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xqb)
            {
                _nqb.Point1 = connector;
                connector.Connected.Add(_nqb);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xqb)
            {
                _nqb.Point2 = connector;
                connector.Connected.Add(_nqb);
            }
        }

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XArcEditor : IEditor, IDisposable
    {
        public enum State { None, Size, StartAngle, SweepAngle }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IArc _xarc;
        private IArc _narc;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XArcEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Arc";
            Key = "A";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    //_xarc.Point2.X = p.X;
                    //_xarc.Point2.Y = p.Y;
                    //_narc.Point2 = _xarc.Point2;
#if CONNECTORS
                    ConnectPoint2(p);
#endif
                    _narc.Bounds.Hide();
                    _canvas.Render(null);
                    _state = State.None;
                    _canvas.ReleaseCapture();
                }
                else
                {
                    _xarc = canvasFactory.CreateArc();
                    _xarc.Point1.X = p.X;
                    _xarc.Point1.Y = p.Y;
                    _xarc.Point2.X = p.X;
                    _xarc.Point2.Y = p.Y;
                    _narc = nativeConverter.Convert(_xarc);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectPoint1(p);
#endif
                    _canvas.Add(_narc);
                    _narc.Bounds = boundsFactory.Create(_canvas, _narc);
                    _narc.Bounds.Update();
                    _narc.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.Size;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_state == State.Size)
                {
                    _xarc.Point2.X = p.X;
                    _xarc.Point2.Y = p.Y;
                    _narc.Point2 = _xarc.Point2;
                    _narc.Bounds.Update();
                    _canvas.Render(null);
                }
            });
        }

#if CONNECTORS

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xarc)
            {
                _narc.Point1 = connector;
                connector.Connected.Add(_narc);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xarc)
            {
                _narc.Point2 = connector;
                connector.Connected.Add(_narc);
            }
        }

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XRectangleEditor : IEditor, IDisposable
    {
        public enum State { None, TopLeft, TopRight, BottomLeft, BottomRight }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IRectangle _xrectangle;
        private IRectangle _nrectangle;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XRectangleEditor(
            INativeConverter nativeConverter, 
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Rectangle";
            Key = "R";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    //_xrectangle.Point2.X = p.X;
                    //_xrectangle.Point2.Y = p.Y;
                    //_nrectangle.Point2 = _xrectangle.Point2;
#if CONNECTORS
                    ConnectPoint2(p); 
#endif
                    _nrectangle.Bounds.Hide();
                    _canvas.Render(null);
                    _state = State.None;
                    _canvas.ReleaseCapture();
                }
                else
                {
                    _xrectangle = canvasFactory.CreateRectangle();
                    _xrectangle.Point1.X = p.X;
                    _xrectangle.Point1.Y = p.Y;
                    _xrectangle.Point2.X = p.X;
                    _xrectangle.Point2.Y = p.Y;
                    _nrectangle = nativeConverter.Convert(_xrectangle);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectPoint1(p);
#endif
                    _canvas.Add(_nrectangle);
                    _nrectangle.Bounds = boundsFactory.Create(_canvas, _nrectangle);
                    _nrectangle.Bounds.Update();
                    _nrectangle.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.BottomRight;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_state == State.BottomRight)
                {
                    _xrectangle.Point2.X = p.X;
                    _xrectangle.Point2.Y = p.Y;
                    _nrectangle.Point2 = _xrectangle.Point2;
                    _nrectangle.Bounds.Update();
                    _canvas.Render(null);
                }
            });
        }

#if CONNECTORS

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xrectangle)
            {
                _nrectangle.Point1 = connector;
                connector.Connected.Add(_nrectangle);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xrectangle)
            {
                _nrectangle.Point2 = connector;
                connector.Connected.Add(_nrectangle);
            }
        } 

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XEllipseEditor : IEditor, IDisposable
    {
        public enum State { None, TopLeft, TopRight, BottomLeft, BottomRight }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IEllipse _xellipse;
        private IEllipse _nellipse;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XEllipseEditor(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Ellipse";
            Key = "E";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    //_xellipse.Point2.X = p.X;
                    //_xellipse.Point2.Y = p.Y;
                    //_nellipse.Point2 = _xellipse.Point2;
#if CONNECTORS
                    ConnectPoint2(p);
#endif
                    _nellipse.Bounds.Hide();
                    _canvas.Render(null);
                    _state = State.None;
                    _canvas.ReleaseCapture();
                }
                else
                {
                    _xellipse = canvasFactory.CreateEllipse();
                    _xellipse.Point1.X = p.X;
                    _xellipse.Point1.Y = p.Y;
                    _xellipse.Point2.X = p.X;
                    _xellipse.Point2.Y = p.Y;
                    _nellipse = nativeConverter.Convert(_xellipse);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectPoint1(p);
#endif
                    _canvas.Add(_nellipse);
                    _nellipse.Bounds = boundsFactory.Create(_canvas, _nellipse);
                    _nellipse.Bounds.Update();
                    _nellipse.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.BottomRight;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_state == State.BottomRight)
                {
                    _xellipse.Point2.X = p.X;
                    _xellipse.Point2.Y = p.Y;
                    _nellipse.Point2 = _xellipse.Point2;
                    _nellipse.Bounds.Update();
                    _canvas.Render(null);
                }
            });
        }

#if CONNECTORS

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xellipse)
            {
                _nellipse.Point1 = connector;
                connector.Connected.Add(_nellipse);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xellipse)
            {
                _nellipse.Point2 = connector;
                connector.Connected.Add(_nellipse);
            }
        }

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XTextEditor : IEditor, IDisposable
    {
        public enum State { None, TopLeft, TopRight, BottomLeft, BottomRight }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Key { get; set; }
        public string Modifiers { get; set; }

        private ICanvas _canvas;
        private IText _xtext;
        private IText _ntext;
        private State _state = State.None;
        private IDisposable _downs;
        private IDisposable _drags;

        public XTextEditor(
            INativeConverter nativeConverter,
            ICanvasFactory canvasFactory,
            IBoundsFactory boundsFactory,
            ICanvas canvas)
        {
            _canvas = canvas;

            Name = "Text";
            Key = "T";
            Modifiers = "";

            var moves = _canvas.Moves.Where(_ => _canvas.IsCaptured);
            var drags = Observable.Merge(_canvas.Downs, _canvas.Ups, moves);

            _downs = _canvas.Downs.Where(_ => IsEnabled).Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_canvas.IsCaptured)
                {
                    //_xtext.Point2.X = p.X;
                    //_xtext.Point2.Y = p.Y;
                    //_ntext.Point2 = _xtext.Point2;
#if CONNECTORS
                    ConnectPoint2(p);
#endif
                    _ntext.Bounds.Hide();
                    _canvas.Render(null);
                    _state = State.None;
                    _canvas.ReleaseCapture();
                }
                else
                {
                    _xtext = canvasFactory.CreateText();
                    _xtext.Point1.X = p.X;
                    _xtext.Point1.Y = p.Y;
                    _xtext.Point2.X = p.X;
                    _xtext.Point2.Y = p.Y;
                    _ntext = nativeConverter.Convert(_xtext);
                    _canvas.History.Snapshot(_canvas);
#if CONNECTORS
                    ConnectPoint1(p);
#endif
                    _canvas.Add(_ntext);
                    _ntext.Bounds = boundsFactory.Create(_canvas, _ntext);
                    _ntext.Bounds.Update();
                    _ntext.Bounds.Show();
                    _canvas.Render(null);
                    _canvas.Capture();
                    _state = State.BottomRight;
                }
            });

            _drags = drags.Where(_ => IsEnabled).Subscribe(p =>
            {
                if (_state == State.BottomRight)
                {
                    _xtext.Point2.X = p.X;
                    _xtext.Point2.Y = p.Y;
                    _ntext.Point2 = _xtext.Point2;
                    _ntext.Bounds.Update();
                    _canvas.Render(null);
                }
            });
        }

#if CONNECTORS

        private void ConnectPoint1(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xtext)
            {
                _ntext.Point1 = connector;
                connector.Connected.Add(_ntext);
            }
        }

        private void ConnectPoint2(Vector2 p)
        {
            var connector = Helper.ConnectAt(_canvas.Children, p.X, p.Y);
            if (connector != null && connector.Connected[0] != _xtext)
            {
                _ntext.Point2 = connector;
                connector.Connected.Add(_ntext);
            }
        }

#endif

        public void Dispose()
        {
            _downs.Dispose();
            _drags.Dispose();
        }
    }

    public class XCanvasFactory : ICanvasFactory
    {
        public IColor CreateColor()
        {
            return new XColor(0x00, 0x00, 0x00, 0x00);
        }

        public IPoint CreatePoint()
        {
            return new XPoint(0.0, 0.0);
        }

        public IPolygon CreatePolygon()
        {
            return new XPolygon();
        }

        public IPin CreatePin()
        {
            var shape = new XEllipse()
            {
                Point1 = new XPoint(-4.0, -4.0),
                Point2 = new XPoint(4.0, 4.0),
                Stroke = new XColor(0x00, 0x00, 0x00, 0x00),
                StrokeThickness = 0.0,
                Fill = new XColor(0xFF, 0x00, 0x00, 0x00)
            };

            var pin = new XPin()
            {
                Point = new XPoint(0.0, 0.0),
                Shape = shape,
            };
            pin.Point.Connected.Add(pin);
            return pin;
        }

        public ILine CreateLine()
        {
            var line = new XLine()
            {
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
            };
            line.Point1.Connected.Add(line);
            line.Point2.Connected.Add(line);
            return line;
        }

        public IBezier CreateBezier()
        {
            var bezier = new XBezier()
            {
                Start = new XPoint(0.0, 0.0),
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                Point3 = new XPoint(0.0, 0.0),
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
                Fill = new XColor(0x00, 0xFF, 0xFF, 0xFF),
                IsFilled = false,
                IsClosed = false
            };
            bezier.Start.Connected.Add(bezier);
            bezier.Point1.Connected.Add(bezier);
            bezier.Point2.Connected.Add(bezier);
            bezier.Point3.Connected.Add(bezier);
            return bezier;
        }

        public IQuadraticBezier CreateQuadraticBezier()
        {
            var quadraticBezier = new XQuadraticBezier()
            {
                Start = new XPoint(0.0, 0.0),
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
                Fill = new XColor(0x00, 0xFF, 0xFF, 0xFF),
                IsFilled = false,
                IsClosed = false
            };
            quadraticBezier.Start.Connected.Add(quadraticBezier);
            quadraticBezier.Point1.Connected.Add(quadraticBezier);
            quadraticBezier.Point2.Connected.Add(quadraticBezier);
            return quadraticBezier;
        }

        public IArc CreateArc()
        {
            var arc = new XArc()
            {
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                StartAngle = 180.0,
                SweepAngle = 180.0,
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
                Fill = new XColor(0x00, 0xFF, 0xFF, 0xFF),
                IsFilled = false,
                IsClosed = false
            };
            arc.Point1.Connected.Add(arc);
            arc.Point2.Connected.Add(arc);
            return arc;
        }

        public IRectangle CreateRectangle()
        {
            var rectangle = new XRectangle()
            {
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
                Fill = new XColor(0x00, 0xFF, 0xFF, 0xFF)
            };
            rectangle.Point1.Connected.Add(rectangle);
            rectangle.Point2.Connected.Add(rectangle);
            return rectangle;
        }

        public IEllipse CreateEllipse()
        {
            var ellipse = new XEllipse()
            {
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                Stroke = new XColor(0xFF, 0x00, 0x00, 0x00),
                StrokeThickness = 2.0,
                Fill = new XColor(0x00, 0xFF, 0xFF, 0xFF)
            };
            ellipse.Point1.Connected.Add(ellipse);
            ellipse.Point2.Connected.Add(ellipse);
            return ellipse;
        }

        public IText CreateText()
        {
            var text = new XText()
            {
                Point1 = new XPoint(0.0, 0.0),
                Point2 = new XPoint(0.0, 0.0),
                HorizontalAlignment = 1,
                VerticalAlignment = 1,
                Size = 11.0,
                Text = "Text",
                Foreground = new XColor(0xFF, 0x00, 0x00, 0x00),
                Backgroud = new XColor(0x00, 0xFF, 0xFF, 0xFF),
            };
            text.Point1.Connected.Add(text);
            text.Point2.Connected.Add(text);
            return text;
        }

        public IBlock CreateBlock()
        {
            return new XBlock();
        }

        public ICanvas CreateCanvas()
        {
            return new XCanvas()
            {
                Width = 600.0,
                Height = 600.0,
                Background = new XColor(0x00, 0xFF, 0xFF, 0xFF),
                SnapX = 15.0,
                SnapY = 15.0,
                EnableSnap = true
            };
        }
    }
}
