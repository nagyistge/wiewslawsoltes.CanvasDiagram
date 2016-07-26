// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfEllipse : IEllipse
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Ellipse _nellipse;
        private IEllipse _xellipse;

        public WpfEllipse(IEllipse ellipse)
        {
            _xellipse = ellipse;

            _strokeBrush = new SolidColorBrush(_xellipse.Stroke.ToNativeColor());
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(_xellipse.Fill.ToNativeColor());
            _fillBrush.Freeze();

            _nellipse = new Ellipse()
            {
                Stroke = _strokeBrush,
                StrokeThickness = ellipse.StrokeThickness,
                Fill = _fillBrush
            };

            Update();

            Native = _nellipse;
        }

        public int Id
        {
            get { return _xellipse.Id; }
            set { _xellipse.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xellipse.Point1; }
            set
            {
                _xellipse.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xellipse.Point2; }
            set
            {
                _xellipse.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xellipse.Point1.X, _xellipse.Point2.X);
            double y = Math.Min(_xellipse.Point1.Y, _xellipse.Point2.Y);
            double width = Math.Abs(_xellipse.Point2.X - _xellipse.Point1.X);
            double height = Math.Abs(_xellipse.Point2.Y - _xellipse.Point1.Y);
            Canvas.SetLeft(_nellipse, x - 1.0);
            Canvas.SetTop(_nellipse, y - 1.0);
            _nellipse.Width = width + 2.0;
            _nellipse.Height = height + 2.0;
        }

        public IColor Stroke
        {
            get { return _xellipse.Stroke; }
            set
            {
                _xellipse.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xellipse.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _nellipse.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xellipse.StrokeThickness; }
            set
            {
                _xellipse.StrokeThickness = value;
                _nellipse.StrokeThickness = value;
            }
        }

        public IColor Fill
        {
            get { return _xellipse.Fill; }
            set
            {
                _xellipse.Fill = value;
                _fillBrush = new SolidColorBrush(_xellipse.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _nellipse.Fill = _fillBrush;
            }
        }
    }
}
