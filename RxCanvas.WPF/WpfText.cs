// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    public class WpfText : IText
    {
        public object Native { get; set; }
        public IBounds Bounds { get; set; }
        private SolidColorBrush _foregroundBrush;
        private SolidColorBrush _backgroundBrush;
        private Grid _grid;
        private TextBlock _tb;
        private IText _xtext;

        public WpfText(IText text)
        {
            _xtext = text;

            _foregroundBrush = new SolidColorBrush(_xtext.Foreground.ToNativeColor());
            _foregroundBrush.Freeze();
            _backgroundBrush = new SolidColorBrush(_xtext.Backgroud.ToNativeColor());
            _backgroundBrush.Freeze();

            _grid = new Grid();
            _grid.Background = _backgroundBrush;

            _tb = new TextBlock();
            _tb.HorizontalAlignment = (HorizontalAlignment)text.HorizontalAlignment;
            _tb.VerticalAlignment = (VerticalAlignment)text.VerticalAlignment;
            _tb.Background = _backgroundBrush;
            _tb.Foreground = _foregroundBrush;
            _tb.FontSize = text.Size;
            _tb.FontFamily = new FontFamily("Calibri");
            _tb.Text = text.Text;

            _grid.Children.Add(_tb);

            Update();

            Native = _grid;
        }

        public int Id
        {
            get { return _xtext.Id; }
            set { _xtext.Id = value; }
        }

        public IPoint Point1
        {
            get { return _xtext.Point1; }
            set
            {
                _xtext.Point1 = value;
                Update();
            }
        }

        public IPoint Point2
        {
            get { return _xtext.Point2; }
            set
            {
                _xtext.Point2 = value;
                Update();
            }
        }

        private void Update()
        {
            double x = Math.Min(_xtext.Point1.X, _xtext.Point2.X);
            double y = Math.Min(_xtext.Point1.Y, _xtext.Point2.Y);
            double width = Math.Abs(_xtext.Point2.X - _xtext.Point1.X);
            double height = Math.Abs(_xtext.Point2.Y - _xtext.Point1.Y);
            Canvas.SetLeft(_grid, x - 1.0);
            Canvas.SetTop(_grid, y - 1.0);
            _grid.Width = width + 2.0;
            _grid.Height = height + 2.0;
        }

        public int HorizontalAlignment
        {
            get { return _xtext.HorizontalAlignment; }
            set
            {
                _xtext.HorizontalAlignment = value;
                _tb.HorizontalAlignment = (HorizontalAlignment)value;
            }
        }

        public int VerticalAlignment
        {
            get { return _xtext.VerticalAlignment; }
            set
            {
                _xtext.VerticalAlignment = value;
                _tb.VerticalAlignment = (VerticalAlignment)value;
            }
        }

        public double Size
        {
            get { return _xtext.Size; }
            set
            {
                _xtext.Size = value;
                _tb.FontSize = value;
            }
        }

        public string Text
        {
            get { return _xtext.Text; }
            set
            {
                _xtext.Text = value;
                _tb.Text = value;
            }
        }

        public IColor Foreground
        {
            get { return _xtext.Foreground; }
            set
            {
                _xtext.Foreground = value;
                _foregroundBrush = new SolidColorBrush(_xtext.Foreground.ToNativeColor());
                _foregroundBrush.Freeze();
                _tb.Foreground = _foregroundBrush;
            }
        }

        public IColor Backgroud
        {
            get { return _xtext.Backgroud; }
            set
            {
                _xtext.Backgroud = value;
                _backgroundBrush = new SolidColorBrush(_xtext.Backgroud.ToNativeColor());
                _backgroundBrush.Freeze();
                _grid.Background = _backgroundBrush;
                _tb.Background = _backgroundBrush;
            }
        }
    }
}
