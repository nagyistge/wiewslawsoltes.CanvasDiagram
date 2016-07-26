// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using RxCanvas.Binary;
using RxCanvas.Bounds;
using RxCanvas.Editors;
using RxCanvas.Interfaces;
using RxCanvas.Model;

namespace RxCanvas
{
    public class Bootstrapper
    {
        public IContainer Build(Assembly[] assembly)
        {
            var builder = new ContainerBuilder();

            // shared editors
            builder.RegisterAssemblyTypes(assembly)
                .As<IEditor>()
                .InstancePerLifetimeScope();

            // shared files
            builder.RegisterAssemblyTypes(assembly)
                .As<IFile>()
                .InstancePerLifetimeScope();

            // shared model
            builder.Register<IModelConverter>(c => new XModelConverter()).SingleInstance();
            builder.Register<ICanvasFactory>(c => new XCanvasFactory()).SingleInstance();

            // native bounds
            builder.Register<IBoundsFactory>(c =>
            {
                var nativeConverter = c.Resolve<INativeConverter>();
                var canvasFactory = c.Resolve<ICanvasFactory>();
                return new BoundsFactory(nativeConverter, canvasFactory);
            }).InstancePerLifetimeScope();

            // native canvas
            builder.Register<ICanvas>(c =>
            {
                var nativeConverter = c.Resolve<INativeConverter>();
                var canvasFactory = c.Resolve<ICanvasFactory>();
                var binaryFile = c.Resolve<IList<IFile>>().Where(e => e.Name == "Binary").FirstOrDefault();
                var xcanvas = canvasFactory.CreateCanvas();
                xcanvas.History = new BinaryHistory(binaryFile);
                return nativeConverter.Convert(xcanvas);
            }).InstancePerLifetimeScope();

            // native modules
            builder.RegisterAssemblyModules(assembly);

            return builder.Build();
        }
    }
}
