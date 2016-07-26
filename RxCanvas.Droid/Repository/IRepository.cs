// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace RxCanvas.Droid
{
    public interface IRepository
    {
        IList<Diagram> GetAll();
        void RemoveAll();
        Diagram Get(int id);
        int Save(Diagram diagram);
        void Delete(Diagram diagram);
    }
}
