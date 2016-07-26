// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Autofac;
using RxCanvas.Droid;
using RxCanvas.Interfaces;

namespace RxCanvas.Views
{
    public class DroidModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<INativeConverter>(c =>
            {
                return new DroidConverter();
            }).SingleInstance();
        }
    }
}
