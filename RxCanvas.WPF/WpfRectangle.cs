// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfRectangle : IRectangle
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Rectangle _nrectangle;
        private IRectangle _xrectangle;

        public WpfRectangle(IRectangle rectangle)
        {
            _xrectangle = rectangle;

            _strokeBrush = new SolidColorBrush(_xrectangle.Stroke.ToNativeColor());
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(_xrectangle.Fill.ToNativeColor());
            _fillBrush.Freeze();

            _nrectangle = new Rectangle()
            {
                Stroke = _strokeBrush,
                StrokeThickness = rectangle.StrokeThickness,
                Fill = _fillBrush
            };

            Update();

            Native = _nrectangle;
        }

        public int Id
        {
            get { return _xrectangle.Id; }
            set { _xrectangle.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xrectangle.Point1; }
            set
            {
                _xrectangle.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xrectangle.Point2; }
            set
            {
                _xrectangle.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xrectangle.Point1.X, _xrectangle.Point2.X);
            double y = Math.Min(_xrectangle.Point1.Y, _xrectangle.Point2.Y);
            double width = Math.Abs(_xrectangle.Point2.X - _xrectangle.Point1.X);
            double height = Math.Abs(_xrectangle.Point2.Y - _xrectangle.Point1.Y);
            Canvas.SetLeft(_nrectangle, x - 1.0);
            Canvas.SetTop(_nrectangle, y - 1.0);
            _nrectangle.Width = width + 2.0;
            _nrectangle.Height = height + 2.0;
        }

        public IColor Stroke
        {
            get { return _xrectangle.Stroke; }
            set
            {
                _xrectangle.Stroke = value;
                _strokeBrush = new SolidColorBrush(_xrectangle.Stroke.ToNativeColor());
                _strokeBrush.Freeze();
                _nrectangle.Stroke = _strokeBrush;
            }
        }

        public double StrokeThickness
        {
            get { return _xrectangle.StrokeThickness; }
            set
            {
                _xrectangle.StrokeThickness = value;
                _nrectangle.StrokeThickness = value;
            }
        }

        public IColor Fill
        {
            get { return _xrectangle.Fill; }
            set
            {
                _xrectangle.Fill = value;
                _fillBrush = new SolidColorBrush(_xrectangle.Fill.ToNativeColor());
                _fillBrush.Freeze();
                _nrectangle.Fill = _fillBrush;
            }
        }
    }
}
