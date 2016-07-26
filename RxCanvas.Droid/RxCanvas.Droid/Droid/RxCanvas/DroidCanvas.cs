// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MathUtil;
using RxCanvas.Interfaces;

namespace RxCanvas.Droid
{
    public class DroidCanvas : ICanvas
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
            get { return 0.0; }
            set { }
        }

        public double Height
        {
            get { return 0.0; }
            set { }
        }

        public IColor Background { get; set; }
        public bool EnableSnap { get; set; }
        public double SnapX { get; set; }
        public double SnapY { get; set; }
        public bool IsCaptured { get; set; }

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        public DroidCanvas(ICanvas canvas)
        {
            Background = canvas.Background;
            SnapX = canvas.SnapX;
            SnapY = canvas.SnapY;
            EnableSnap = canvas.EnableSnap;

            History = canvas.History;

            Children = new ObservableCollection<INative>();

            Native = null;
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
            if (Native != null)
            {
                (Native as CanvasView).Render();
            }
        }
    }
}
