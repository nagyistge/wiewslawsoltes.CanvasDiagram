// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Autofac;
using RxCanvas.Interfaces;
using RxCanvas.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxCanvas.Views
{
    public class WinFormsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<INativeConverter>(c =>
            {
                var panel = new WinFormsCanvasPanel();
                panel.Anchor = System.Windows.Forms.AnchorStyles.None;
                panel.Location = new System.Drawing.Point(100, 12);
                panel.Name = "canvasPanel";
                panel.Size = new System.Drawing.Size(600, 600);
                panel.TabIndex = 0;
                return new WinFormsConverter(panel);
            }).SingleInstance();
        }
    }
}
