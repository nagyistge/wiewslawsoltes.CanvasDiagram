// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MathUtil;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfCanvas : ICanvas
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        public IObservable<Vector2> Downs { get; set; }
        public IObservable<Vector2> Ups { get; set; }
        public IObservable<Vector2> Moves { get; set; }
        private SolidColorBrush _backgroundBrush;
        private ICanvas _xcanvas;
        private Canvas _ncanvas;

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public WpfCanvas(ICanvas canvas)
        {
            _xcanvas = canvas;

            _backgroundBrush = new SolidColorBrush(_xcanvas.Background.ToNativeColor());
            _backgroundBrush.Freeze();

            _ncanvas = new Canvas()
            {
                Width = canvas.Width,
                Height = canvas.Height,
                Background = _backgroundBrush
            };

            Downs = Observable.FromEventPattern<MouseButtonEventArgs>(
                _ncanvas,
                "PreviewMouseLeftButtonDown").Select(e =>
                {
                    var p = e.EventArgs.GetPosition(_ncanvas);
                    return new Vector2(
                        _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                        _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
                });

            Ups = Observable.FromEventPattern<MouseButtonEventArgs>(
                _ncanvas,
                "PreviewMouseLeftButtonUp").Select(e =>
                {
                    var p = e.EventArgs.GetPosition(_ncanvas);
                    return new Vector2(
                        _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                        _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
                });

            Moves = Observable.FromEventPattern<MouseEventArgs>(
                _ncanvas,
                "PreviewMouseMove").Select(e =>
                {
                    var p = e.EventArgs.GetPosition(_ncanvas);
                    return new Vector2(
                        _xcanvas.EnableSnap ? Snap(p.X, _xcanvas.SnapX) : p.X,
                        _xcanvas.EnableSnap ? Snap(p.Y, _xcanvas.SnapY) : p.Y);
                });

            Native = _ncanvas;
        }

        public int Id
        {
            get { return _xcanvas.Id; }
            set { _xcanvas.Id = value; }
        }

        public IHistory History
        {
            get { return _xcanvas.History; }
            set { _xcanvas.History = value; }
        }

        public IList<INative> Children
        {
            get { return _xcanvas.Children; }
            set { _xcanvas.Children = value; }
        }

        public double Width
        {
            get { return _ncanvas.Width; }
            set { _ncanvas.Width = value; }
        }

        public double Height
        {
            get { return _ncanvas.Height; }
            set { _ncanvas.Height = value; }
        }

        public IColor Background
        {
            get { return _xcanvas.Background; }
            set
            {
                _xcanvas.Background = value;
                if (_xcanvas.Background == null)
                {
                    _backgroundBrush = null;
                }
                else
                {
                    _backgroundBrush = new SolidColorBrush(_xcanvas.Background.ToNativeColor());
                    _backgroundBrush.Freeze();
                }
                _ncanvas.Background = _backgroundBrush;
            }
        }

        public bool EnableSnap
        {
            get { return _xcanvas.EnableSnap; }
            set { _xcanvas.EnableSnap = value; }
        }

        public double SnapX
        {
            get { return _xcanvas.SnapX; }
            set { _xcanvas.SnapX = value; }
        }

        public double SnapY
        {
            get { return _xcanvas.SnapY; }
            set { _xcanvas.SnapY = value; }
        }

        public bool IsCaptured
        {
            get { return Mouse.Captured == _ncanvas; }
            set { _ncanvas.CaptureMouse(); }
        }

        public void Capture()
        {
            _ncanvas.CaptureMouse();
        }

        public void ReleaseCapture()
        {
            _ncanvas.ReleaseMouseCapture();
        }

        public void Add(INative value)
        {
            if (value.Native != null)
            {
                _ncanvas.Children.Add(value.Native as UIElement);
            }

            Children.Add(value);
        }

        public void Remove(INative value)
        {
            if (value.Native != null)
            {
                _ncanvas.Children.Remove(value.Native as UIElement);
            }

            Children.Remove(value);
        }

        public void Clear()
        {
            _ncanvas.Children.Clear();
            Children.Clear();
        }

        public void Render(INative context)
        {
        }
    }
}
