// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Windows.Forms;

namespace RxCanvas.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var v = new MathUtil.Vector2();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
