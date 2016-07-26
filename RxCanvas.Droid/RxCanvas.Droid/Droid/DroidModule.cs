// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Autofac;
using RxCanvas.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxCanvas.Droid;

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
