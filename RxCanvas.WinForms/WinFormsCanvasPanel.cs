// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using RxCanvas.Interfaces;

namespace RxCanvas.WinForms
{
    public class WinFormsCanvasPanel : Panel
    {
        public IList<ICanvas> Layers { get; set; }

        public double Zoom { get; set; }
        public double PanX { get; set; }
        public double PanY { get; set; }

        private Point previous;

        public WinFormsCanvasPanel()
        {
            this.SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.SupportsTransparentBackColor,
                true);

            this.BackColor = Color.Transparent;

            this.Layers = new List<ICanvas>();

            this.Zoom = 1.0;
            this.PanX = 0.0;
            this.PanY = 0.0;

            this.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    previous = e.Location;
                }
            };

            this.MouseMove += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    Point p = e.Location;
                    float dx = p.X - previous.X;
                    float dy = p.Y - previous.Y;
                    previous = p;
                    this.PanX += dx;
                    this.PanY += dy;
                    this.Invalidate();
                }
            };

            this.MouseWheel += (sender, e) =>
            {
                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && this.Zoom < .4)
                    return;
                Point relative = e.Location;
                double abosuluteX = relative.X * this.Zoom + this.PanX;
                double abosuluteY = relative.Y * this.Zoom + this.PanY;
                this.Zoom += zoom;
                this.PanX = abosuluteX - relative.X * this.Zoom;
                this.PanY = abosuluteY - relative.Y * this.Zoom;
                this.Invalidate();
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw(e.Graphics, Layers);
        }

        private Color ToNativeColor(IColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void Draw(Graphics g, IList<ICanvas> layers)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // pan
            g.TranslateTransform(
                (float)PanX,
                (float)PanY);

            // zoom
            g.ScaleTransform(
                (float)Zoom,
                (float)Zoom);

            // background
            g.Clear(ToNativeColor(layers.FirstOrDefault().Background));

            // layers
            for (int i = 0; i < layers.Count; i++)
            {
                DrawChildren(g, layers[i].Children);
            }
        }

        private void DrawChildren(Graphics g, IList<INative> children)
        {
            foreach (var child in children)
            {
                if (child is IPin)
                {
                    var pin = child as IPin;
                    var color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                    Brush brush = new SolidBrush(color);

                    double x = pin.Point.X - 4.0;
                    double y = pin.Point.Y - 4.0;
                    double width = 8.0;
                    double height = 8.0;

                    g.FillEllipse(
                        brush,
                        (float)x,
                        (float)y,
                        (float)width,
                        (float)height);

                    brush.Dispose();
                }
                else if (child is ILine)
                {
                    var line = child as ILine;
                    Pen pen = new Pen(
                        ToNativeColor(line.Stroke),
                        (float)line.StrokeThickness);

                    g.DrawLine(
                        pen,
                        (float)line.Point1.X,
                        (float)line.Point1.Y,
                        (float)line.Point2.X,
                        (float)line.Point2.Y);

                    pen.Dispose();
                }
                else if (child is IBezier)
                {
                    var bezier = child as IBezier;
                    Pen pen = new Pen(
                        ToNativeColor(bezier.Stroke),
                        (float)bezier.StrokeThickness);

                    g.DrawBezier(
                        pen,
                        (float)bezier.Start.X,
                        (float)bezier.Start.Y,
                        (float)bezier.Point1.X,
                        (float)bezier.Point1.Y,
                        (float)bezier.Point2.X,
                        (float)bezier.Point2.Y,
                        (float)bezier.Point3.X,
                        (float)bezier.Point3.Y);

                    pen.Dispose();
                }
                else if (child is IQuadraticBezier)
                {
                    var quadraticBezier = child as IQuadraticBezier;
                    Pen pen = new Pen(
                        ToNativeColor(quadraticBezier.Stroke),
                        (float)quadraticBezier.StrokeThickness);

                    double x1 = quadraticBezier.Start.X;
                    double y1 = quadraticBezier.Start.Y;
                    double x2 = quadraticBezier.Start.X + (2.0 * (quadraticBezier.Point1.X - quadraticBezier.Start.X)) / 3.0;
                    double y2 = quadraticBezier.Start.Y + (2.0 * (quadraticBezier.Point1.Y - quadraticBezier.Start.Y)) / 3.0;
                    double x3 = x2 + (quadraticBezier.Point2.X - quadraticBezier.Start.X) / 3.0;
                    double y3 = y2 + (quadraticBezier.Point2.Y - quadraticBezier.Start.Y) / 3.0;
                    double x4 = quadraticBezier.Point2.X;
                    double y4 = quadraticBezier.Point2.Y;

                    g.DrawBezier(
                        pen,
                        (float)x1,
                        (float)y1,
                        (float)x2,
                        (float)y2,
                        (float)x3,
                        (float)y3,
                        (float)x4,
                        (float)y4);

                    pen.Dispose();
                }
                else if (child is IArc)
                {
                    var arc = child as IArc;

                    double x = Math.Min(arc.Point1.X, arc.Point2.X);
                    double y = Math.Min(arc.Point1.Y, arc.Point2.Y);
                    double width = Math.Abs(arc.Point2.X - arc.Point1.X);
                    double height = Math.Abs(arc.Point2.Y - arc.Point1.Y);

                    if (width > 0.0 && height > 0.0)
                    {
                        Pen pen = new Pen(
                            ToNativeColor(arc.Stroke),
                            (float)arc.StrokeThickness);

                        g.DrawArc(
                            pen,
                            (float)x,
                            (float)y,
                            (float)width,
                            (float)height,
                            (float)arc.StartAngle,
                            (float)arc.SweepAngle);

                        pen.Dispose();
                    }
                }
                else if (child is IRectangle)
                {
                    var rectangle = child as IRectangle;
                    Pen pen = new Pen(
                        ToNativeColor(rectangle.Stroke),
                        (float)rectangle.StrokeThickness);

                    double x = Math.Min(rectangle.Point1.X, rectangle.Point2.X);
                    double y = Math.Min(rectangle.Point1.Y, rectangle.Point2.Y);
                    double width = Math.Abs(rectangle.Point2.X - rectangle.Point1.X);
                    double height = Math.Abs(rectangle.Point2.Y - rectangle.Point1.Y);

                    g.DrawRectangle(
                        pen,
                        (float)x,
                        (float)y,
                        (float)width,
                        (float)height);

                    pen.Dispose();
                }
                else if (child is IEllipse)
                {
                    var ellipse = child as IEllipse;
                    Pen pen = new Pen(
                        ToNativeColor(ellipse.Stroke),
                        (float)ellipse.StrokeThickness);

                    double x = Math.Min(ellipse.Point1.X, ellipse.Point2.X);
                    double y = Math.Min(ellipse.Point1.Y, ellipse.Point2.Y);
                    double width = Math.Abs(ellipse.Point2.X - ellipse.Point1.X);
                    double height = Math.Abs(ellipse.Point2.Y - ellipse.Point1.Y);

                    g.DrawEllipse(
                        pen,
                        (float)x,
                        (float)y,
                        (float)width,
                        (float)height);

                    pen.Dispose();
                }
                else if (child is IText)
                {
                    var text = child as IText;
                    Brush brush = new SolidBrush(ToNativeColor(text.Foreground));
                    Font font = new Font("Calibri", (float)(text.Size * 72 / 96));

                    double x = Math.Min(text.Point1.X, text.Point2.X);
                    double y = Math.Min(text.Point1.Y, text.Point2.Y);
                    double width = Math.Abs(text.Point2.X - text.Point1.X);
                    double height = Math.Abs(text.Point2.Y - text.Point1.Y);

                    g.DrawString(
                        text.Text,
                        font,
                        brush,
                        new RectangleF(
                            (float)x,
                            (float)y,
                            (float)width,
                            (float)height),
                        new StringFormat()
                        {
                            Alignment = (StringAlignment)text.HorizontalAlignment,
                            LineAlignment = (StringAlignment)text.VerticalAlignment
                        });

                    brush.Dispose();
                    font.Dispose();
                }
                else if (child is IBlock)
                {
                    var block = child as IBlock;

                    DrawChildren(g, block.Children);
                }
            }
        }
    }
}
