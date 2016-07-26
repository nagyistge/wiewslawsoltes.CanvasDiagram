// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using MathUtil;
using RxCanvas.Interfaces;

namespace RxCanvas.WinForms
{
    public class WinFormsCanvas : ICanvas
    {
        public int Id { get; set; }
        public object Native { get; set; }
        public IBounds Bounds { get; set; }

        public IObservable<Vector2> Downs { get; set; }
        public IObservable<Vector2> Ups { get; set; }
        public IObservable<Vector2> Moves { get; set; }

        public IHistory History { get; set; }

        public IList<INative> Children { get; set; }

        public double Width 
        {
            get { return _panel.Width; }
            set { _panel.Width = (int)value; }
        }

        public double Height
        {
            get { return _panel.Height; }
            set { _panel.Height = (int)value; }
        }

        public IColor Background { get; set; }

        public double Zoom { get; set; }
        public double PanX { get; set; }
        public double PanY { get; set; }

        public bool EnableSnap { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }

        public bool IsCaptured { get; set; }

        private WinFormsCanvasPanel _panel;

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public WinFormsCanvas(ICanvas canvas, WinFormsCanvasPanel panel)
        {
            Background = canvas.Background;
            SnapX = canvas.SnapX;
            SnapY = canvas.SnapY;
            EnableSnap = canvas.EnableSnap;

            History = canvas.History;

            Children = new List<INative>();

            _panel = panel;
            _panel.Layers.Add(this);

            Downs = Observable.FromEventPattern<MouseEventArgs>(_panel, "MouseDown")
                .Where(e => e.EventArgs.Button == MouseButtons.Left)
                .Select(e => Snap(e.EventArgs.Location));

            Ups = Observable.FromEventPattern<MouseEventArgs>(_panel, "MouseUp")
                .Where(e => e.EventArgs.Button == MouseButtons.Left)
                .Select(e => Snap(e.EventArgs.Location));

            Moves = Observable.FromEventPattern<MouseEventArgs>(_panel, "MouseMove")
                .Select(e => Snap(e.EventArgs.Location));

            Native = _panel;
        }

        private Vector2 Snap(Point p)
        {
            double x = EnableSnap ?
                Snap((double)(p.X) - _panel.PanX, SnapX * _panel.Zoom) : 
                (double)(p.X) - _panel.PanX;

            double y = EnableSnap ?
                Snap((double)(p.Y) - _panel.PanY, SnapY * _panel.Zoom) : 
                (double)(p.Y) - _panel.PanY;

            x /= _panel.Zoom;
            y /= _panel.Zoom;

            return new Vector2(x, y);
        }

        public void Capture()
        {
            IsCaptured = true;
        }

        public void ReleaseCapture()
        {
            IsCaptured = false;
        }

        public void Add(INative value)
        {
            Children.Add(value);
        }

        public void Remove(INative value)
        {
            Children.Remove(value);
        }

        public void Clear()
        {
            Children.Clear();
        }

        public void Render(INative context)
        {
            _panel.Invalidate();
        }
    }
}
