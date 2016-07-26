// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using RxCanvas.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxCanvas.Creators
{
    public class DxfCreator : ICreator
    {
        public string Name { get; set; }
        public string Extension { get; set; }

        public DxfCreator()
        {
            Name = "Dxf";
            Extension = "dxf";
        }

        public void Save(string path, ICanvas canvas)
        {
            throw new NotImplementedException();
        }

        public void Save(string path, IEnumerable<ICanvas> canvases)
        {
            throw new NotImplementedException();
        }
    }
}
