// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using RxCanvas.Interfaces;

namespace RxCanvas
{
    public class DrawingView
    {
        public IList<ICanvas> Layers { get; set; }

        public IList<IEditor> Editors { get; set; }
        public IList<IFile> Files { get; set; }
        public IList<ICreator> Creators { get; set; }

        private IList<ILifetimeScope> _scopes;

        public DrawingView(Assembly[] assembly)
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Build(assembly);

            // create scopes
            _scopes = new List<ILifetimeScope>();
            _scopes.Add(container.BeginLifetimeScope());
            _scopes.Add(container.BeginLifetimeScope());

            // create layers
            Layers = new List<ICanvas>();
            for (int i = 0; i < _scopes.Count; i++)
            {
                Layers.Add(_scopes[i].Resolve<ICanvas>());
            }
        }

        public void Initialize()
        {
            // drawing layer
            var scope = _scopes.LastOrDefault();
            Editors = scope.Resolve<IList<IEditor>>();
            Files = scope.Resolve<IList<IFile>>();
            Creators = scope.Resolve<IList<ICreator>>();

            // default editor
            Editors.Where(e => e.Name == "Line")
                .FirstOrDefault()
                .IsEnabled = true;
        }

        public void Open(string path, int index)
        {
            var xcanvas = Files[index].Open(path);
            ToNative(xcanvas);
        }

        public void Save(string path, int index)
        {
            var xcanvas = ToModel();
            Files[index].Save(path, xcanvas);
        }

        public void Export(string path, int index)
        {
            var xcanvas = ToModel();
            Creators[index].Save(path, xcanvas);
        }

        public void Enable(IEditor editor)
        {
            for (int i = 0; i < Editors.Count; i++)
            {
                Editors[i].IsEnabled = false;
            }

            if (editor != null)
            {
                editor.IsEnabled = true;
            }
        }

        public void ToggleSnap()
        {
            var drawingCanvas = Layers.LastOrDefault();
            drawingCanvas.EnableSnap = drawingCanvas.EnableSnap ? false : true;
        }

        public void Undo()
        {
            var drawingCanvas = Layers.LastOrDefault();
            var xcanvas = drawingCanvas.History.Undo(drawingCanvas);
            if (xcanvas != null)
            {
                ToNative(xcanvas);
                Render();
            }
        }

        public void Redo()
        {
            var drawingCanvas = Layers.LastOrDefault();
            var xcanvas = drawingCanvas.History.Redo(drawingCanvas);
            if (xcanvas != null)
            {
                ToNative(xcanvas);
                Render();
            }
        }

        public void Clear()
        {
            var drawingCanvas = Layers.LastOrDefault();
            drawingCanvas.History.Snapshot(drawingCanvas);
            drawingCanvas.Clear();
            drawingCanvas.Render(null);
        }

        public void Render()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Render(null);
            }
        }

        public void CreateGrid(
            double width, 
            double height, 
            double size, 
            double originX, 
            double originY)
        {
            var scope = _scopes.FirstOrDefault();
            var backgroundCanvas = scope.Resolve<ICanvas>();
            var nativeConverter = scope.Resolve<INativeConverter>();
            var canvasFactory = scope.Resolve<ICanvasFactory>();

            double thickness = 2.0;

            var stroke = canvasFactory.CreateColor();
            stroke.A = 0xFF;
            stroke.R = 0xE8;
            stroke.G = 0xE8;
            stroke.B = 0xE8;

            // horizontal
            for (double y = size; y < height; y += size)
            {
                var xline = canvasFactory.CreateLine();
                xline.Point1.X = originX;
                xline.Point1.Y = y;
                xline.Point2.X = width;
                xline.Point2.Y = y;
                xline.Stroke = stroke;
                xline.StrokeThickness = thickness;
                var nline = nativeConverter.Convert(xline);
                backgroundCanvas.Add(nline);
            }

            // vertical lines
            for (double x = size; x < width; x += size)
            {
                var xline = canvasFactory.CreateLine();
                xline.Point1.X = x;
                xline.Point1.Y = originY;
                xline.Point2.X = x;
                xline.Point2.Y = height;
                xline.Stroke = stroke;
                xline.StrokeThickness = thickness;
                var nline = nativeConverter.Convert(xline);
                backgroundCanvas.Add(nline);
            }
        }

        public ICanvas ToModel()
        {
#if false
            var scope = _scopes.LastOrDefault();
            var drawingCanvas = scope.Resolve<ICanvas>();
            var modelConverter = scope.Resolve<IModelConverter>();
            return modelConverter.Convert(drawingCanvas);
#else
            var scope = _scopes.LastOrDefault();
            return scope.Resolve<ICanvas>();
#endif
        }

        public INative ToNative(ICanvas xcanvas)
        {
            var scope = _scopes.LastOrDefault();
            var nativeConverter = scope.Resolve<INativeConverter>();
            var canvasFactory = scope.Resolve<ICanvasFactory>();
            var nativeCanvas = scope.Resolve<ICanvas>();
            var boundsFactory = scope.Resolve<IBoundsFactory>();

            nativeCanvas.Clear();

            var natives = ToNatives(
                nativeConverter,
                boundsFactory,
                nativeCanvas,
                xcanvas.Children);

            foreach (var native in natives)
            {
                nativeCanvas.Add(native);
            }

            return nativeCanvas;
        }

        private IList<INative> ToNatives(
            INativeConverter nativeConverter,
            IBoundsFactory boundsFactory,
            ICanvas nativeCanvas,
            IList<INative> xchildren)
        {
            var natives = new List<INative>();

            foreach (var child in xchildren)
            {
                if (child is IPin)
                {
                    var native = nativeConverter.Convert(child as IPin);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is ILine)
                {
                    var native = nativeConverter.Convert(child as ILine);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IBezier)
                {
                    var native = nativeConverter.Convert(child as IBezier);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IQuadraticBezier)
                {
                    var native = nativeConverter.Convert(child as IQuadraticBezier);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IArc)
                {
                    var native = nativeConverter.Convert(child as IArc);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IRectangle)
                {
                    var native = nativeConverter.Convert(child as IRectangle);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IEllipse)
                {
                    var native = nativeConverter.Convert(child as IEllipse);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IText)
                {
                    var native = nativeConverter.Convert(child as IText);
                    natives.Add(native);
                    native.Bounds = boundsFactory.Create(nativeCanvas, native);
                    if (native.Bounds != null)
                    {
                        native.Bounds.Update();
                    }
                }
                else if (child is IBlock)
                {
                    var native = nativeConverter.Convert(child as IBlock);
                    natives.Add(native);
                    var blockNatives = ToNatives(
                        nativeConverter,
                        boundsFactory,
                        nativeCanvas,
                        native.Children);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return natives;
        }

        public void CreateBlock()
        {
            var scope = _scopes.LastOrDefault();
            var nativeConverter = scope.Resolve<INativeConverter>();
            var canvasFactory = scope.Resolve<ICanvasFactory>();
            var drawingCanvas = scope.Resolve<ICanvas>();
            var boundsFactory = scope.Resolve<IBoundsFactory>();

            var selected = drawingCanvas
                .Children
                .Where(c => c.Bounds != null && c.Bounds.IsVisible())
                .ToList();

            foreach(var child in selected)
            {
                child.Bounds.Hide();
            }
            drawingCanvas.History.Snapshot(drawingCanvas);

            var xblock = canvasFactory.CreateBlock();

            foreach(var child in selected)
            {
                xblock.Children.Add(child);
                drawingCanvas.Children.Remove(child);
            }

            var nblock = nativeConverter.Convert(xblock);
            drawingCanvas.Children.Add(nblock);
            drawingCanvas.Render(null);
        }

        public void Delete()
        {
            var scope = _scopes.LastOrDefault();
            var drawingCanvas = scope.Resolve<ICanvas>();

            var selected = drawingCanvas
                .Children
                .Where(c => c.Bounds != null && c.Bounds.IsVisible())
                .ToList();

            foreach (var child in selected)
            {
                child.Bounds.Hide();
            }
            drawingCanvas.History.Snapshot(drawingCanvas);

            foreach (var child in selected)
            {
                drawingCanvas.Children.Remove(child);
            }

            drawingCanvas.Render(null);
        }
    }
}
