// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.Win32;
using RxCanvas.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RxCanvas.WPF
{
    public partial class MainWindow : Window
    {
        private DrawingView _view;
        private IDictionary<Tuple<Key, ModifierKeys>, Action> _shortcuts;

        public MainWindow()
        {
            InitializeComponent();

            InitializeDrawingView();
            InitlializeShortucts();
            Initialize();
        }

        private void InitializeDrawingView()
        {
            _view = new DrawingView();
            _view.Initialize();
        }

        private void InitlializeShortucts()
        {
            // shortcuts dictionary
            _shortcuts = new Dictionary<Tuple<Key, ModifierKeys>, Action>();

            // key converters
            var keyConverter = new KeyConverter();
            var modifiersKeyConverter = new ModifierKeysConverter();

            // open shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("O"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => Open());

            // save shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("S"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => Save());

            // export shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("E"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => Export());

            // undo shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("Z"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => _view.Undo());

            // redo shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("Y"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => _view.Redo());

            // snap shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("S"),
                    ModifierKeys.None),
                () => _view.ToggleSnap());

            // clear shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("Delete"),
                    (ModifierKeys)modifiersKeyConverter.ConvertFromString("Control")),
                () => _view.Clear());

            // editor shortcuts
            foreach (var editor in _view.Editors)
            {
                var _editor = editor;
                _shortcuts.Add(
                    new Tuple<Key, ModifierKeys>(
                        (Key)keyConverter.ConvertFromString(editor.Key),
                        editor.Modifiers == "" ? ModifierKeys.None : (ModifierKeys)modifiersKeyConverter.ConvertFromString(editor.Modifiers)),
                    () => _view.Enable(_editor));
            }

            // block shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("G"),
                    ModifierKeys.None),
                () => _view.CreateBlock());

            // delete shortcut
            _shortcuts.Add(
                new Tuple<Key, ModifierKeys>(
                    (Key)keyConverter.ConvertFromString("Delete"),
                    ModifierKeys.None),
                () => _view.Delete());
        }

        private void Initialize()
        {
            // add canvas layers to root layout
            for (int i = 0; i < _view.Layers.Count; i++)
            {
                Layout.Children.Add(_view.Layers[i].Native as UIElement);
            }

            // create grid canvas
            _view.CreateGrid(600.0, 600.0, 30.0, 0.0, 0.0);

            // handle keyboard input
            PreviewKeyDown += (sender, e) =>
            {
                Action action;
                bool result = _shortcuts.TryGetValue(
                    new Tuple<Key, ModifierKeys>(e.Key, Keyboard.Modifiers),
                    out action);

                if (result == true && action != null)
                {
                    action();
                }
            };

            // set data context
            DataContext = _view.Layers.LastOrDefault();
        }

        private string ToFileFilter()
        {
            bool first = true;
            string filter = string.Empty;
            foreach (var serializer in _view.Files)
            {
                filter += string.Format(
                    "{0}{1} File (*.{2})|*.{2}",
                    first == false ? "|" : string.Empty,
                    serializer.Name,
                    serializer.Extension);

                if (first == true)
                {
                    first = false;
                }
            }
            return filter;
        }

        private string ToCreatorFilter()
        {
            bool first = true;
            string filter = string.Empty;
            foreach (var creator in _view.Creators)
            {
                filter += string.Format(
                    "{0}{1} File (*.{2})|*.{2}",
                    first == false ? "|" : string.Empty,
                    creator.Name,
                    creator.Extension);

                if (first == true)
                {
                    first = false;
                }
            }
            return filter;
        }

        private void Open()
        {
            string filter = ToFileFilter();
            int defaultFilterIndex = _view.Files
                .IndexOf(_view.Files.Where(c => c.Name == "Binary")
                .FirstOrDefault()) + 1;

            var dlg = new OpenFileDialog()
            {
                Filter = filter,
                FilterIndex = defaultFilterIndex
            };

            if (dlg.ShowDialog(this) == true)
            {
                string path = dlg.FileName;
                int filterIndex = dlg.FilterIndex;
                _view.Open(path, filterIndex - 1);
            }
        }

        private void Save()
        {
            string filter = ToFileFilter();
            int defaultFilterIndex = _view.Files
                .IndexOf(_view.Files.Where(c => c.Name == "Binary")
                .FirstOrDefault()) + 1;

            var dlg = new SaveFileDialog()
            {
                Filter = filter,
                FilterIndex = defaultFilterIndex,
                FileName = "canvas"
            };

            if (dlg.ShowDialog(this) == true)
            {
                string path = dlg.FileName;
                int filterIndex = dlg.FilterIndex;
                _view.Save(path, filterIndex - 1);
            }
        }

        private void Export()
        {
            string filter = ToCreatorFilter();
            int defaultFilterIndex = _view.Creators
                .IndexOf(_view.Creators.Where(c => c.Name == "Pdf")
                .FirstOrDefault()) + 1;

            var dlg = new SaveFileDialog()
            {
                Filter = filter,
                FilterIndex = defaultFilterIndex,
                FileName = "canvas"
            };

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                int filterIndex = dlg.FilterIndex;
                _view.Export(path, filterIndex - 1);
            }
        }
    }
}
