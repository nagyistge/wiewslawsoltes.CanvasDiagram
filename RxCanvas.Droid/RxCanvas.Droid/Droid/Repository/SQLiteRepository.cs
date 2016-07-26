// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using SQLite;

namespace RxCanvas.Droid
{
    public class SQLiteRepository : IRepository
    {
        private readonly SQLiteConnection _connection;

        public SQLiteRepository(string path)
        {
            _connection = new SQLiteConnection(path);
            _connection.CreateTable<Diagram>();
        }

        public IList<Diagram> GetAll()
        {
            return _connection
                .Table<Diagram>()
                .OrderBy(x => x.Id)
                .ToList();
        }

        public void RemoveAll()
        {
            foreach (var diagram in _connection.Table<Diagram> ())
            {
                _connection.Delete(diagram);
            }
        }

        public Diagram Get(int id)
        {
            return _connection
                .Table<Diagram>()
                .FirstOrDefault(x => x.Id == id);
        }

        public int Save(Diagram diagram)
        {
            if (diagram.Id != 0)
            {
                _connection.Update(diagram);
                return diagram.Id;
            }
            else
            {
                return _connection.Insert(diagram);
            }
        }

        public void Delete(Diagram diagram)
        {
            if (diagram.Id != 0)
            {
                _connection.Delete(diagram);
            }
        }
    }
}
