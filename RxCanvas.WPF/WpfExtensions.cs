// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows.Media;
using RxCanvas.Interfaces;

namespace RxCanvas.WPF
{
    internal static class WpfExtensions
    {
        public static Color ToNativeColor(this IColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
