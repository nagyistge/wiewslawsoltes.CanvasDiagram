// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfPin : IPin
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _strokeBrush;
        private SolidColorBrush _fillBrush;
        private Ellipse _nellipse;
        private double _size;
        private IPin _xpin;

        public WpfPin(IPin pin)
        {
            _xpin = pin;

            _size = 8.0;

            _strokeBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            _strokeBrush.Freeze();
            _fillBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            _fillBrush.Freeze();

            _nellipse = new Ellipse()
            {
                Stroke = _strokeBrush,
                StrokeThickness = 2.0,
                Fill = _fillBrush
            };

            Update();

            Native = _nellipse;
        }

        public int Id
        {
            get { return _xpin.Id; }
            set { _xpin.Id = value; }
        }

        public INative Shape
        {
            get { return _xpin.Shape; }
            set
            {
                _xpin.Shape = value;
                Update();
            }
        }

        public IPoint Point
        {
            get { return _xpin.Point; }
            set
            {
                _xpin.Point = value;
                Update();
            }
        }

        private void Update()
        {
            double hsize = _size / 2.0;
            Canvas.SetLeft(_nellipse, _xpin.Point.X - hsize);
            Canvas.SetTop(_nellipse, _xpin.Point.Y - hsize);
            _nellipse.Width = _size;
            _nellipse.Height = _size;
        }
    }
}
