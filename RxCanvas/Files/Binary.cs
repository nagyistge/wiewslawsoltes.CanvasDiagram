// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RxCanvas.Interfaces;
using RxCanvas.Model;

namespace RxCanvas.Serializers
{
    internal enum NativeType : byte
    {
        // Solution
        Solution        = 0x01,
        Project         = 0x02,
        Canvas          = 0x03,
        // Block
        Block           = 0x11,
        End             = 0x12,
        // Primitive
        Pin             = 0x21,
        Line            = 0x22,
        Bezier          = 0x23,
        QuadraticBezier = 0x24,
        Arc             = 0x25,
        Rectangle       = 0x26,
        Ellipse         = 0x27,
        Text            = 0x28,
    }

    internal struct BPoint
    {
        public int Id;
        public int[] Connected;
        public IPoint Point;
    }

    internal class IdReset
    {
        private void Reset(IPoint point)
        {
            point.Id = 0;
        }

        private void Reset(IPin pin)
        {
            pin.Id = 0;
            Reset(pin.Point);
            Reset(pin.Shape);
        }

        private void Reset(ILine line)
        {
            line.Id = 0;
            Reset(line.Point1);
            Reset(line.Point2);
        }

        private void Reset(IBezier bezier)
        {
            bezier.Id = 0;
            Reset(bezier.Start);
            Reset(bezier.Point1);
            Reset(bezier.Point2);
            Reset(bezier.Point3);
        }

        private void Reset(IQuadraticBezier quadraticBezier)
        {
            quadraticBezier.Id = 0;
            Reset(quadraticBezier.Start);
            Reset(quadraticBezier.Point1);
            Reset(quadraticBezier.Point2);
        }

        private void Reset(IArc arc)
        {
            arc.Id = 0;
            Reset(arc.Point1);
            Reset(arc.Point2);
        }

        private void Reset(IRectangle rectangle)
        {
            rectangle.Id = 0;
            Reset(rectangle.Point1);
            Reset(rectangle.Point2);
        }

        private void Reset(IEllipse ellipse)
        {
            ellipse.Id = 0;
            Reset(ellipse.Point1);
            Reset(ellipse.Point2);
        }

        private void Reset(IText text)
        {
            text.Id = 0;
            Reset(text.Point1);
            Reset(text.Point2);
        }

        private void Reset(IList<INative> children)
        {
            int count = children.Count;
            for (int i = 0; i < count; i++)
            {
                Reset(children[i]);
            }
        }

        private void Reset(INative child)
        {
            if (child is IPin)
            {
                Reset(child as IPin);
            }
            else if (child is ILine)
            {
                Reset(child as ILine);
            }
            else if (child is IBezier)
            {
                Reset(child as IBezier);
            }
            else if (child is IQuadraticBezier)
            {
                Reset(child as IQuadraticBezier);
            }
            else if (child is IArc)
            {
                Reset(child as IArc);
            }
            else if (child is IRectangle)
            {
                Reset(child as IRectangle);
            }
            else if (child is IEllipse)
            {
                Reset(child as IEllipse);
            }
            else if (child is IText)
            {
                Reset(child as IText);
            }
            else if (child is IBlock)
            {
                Reset(child as IBlock);
            }
        }

        private void Reset(IBlock block)
        {
            block.Id = 0;
            Reset(block.Children);
        }

        public void Reset(ICanvas canvas)
        {
            canvas.Id = 0;
            Reset(canvas.Children);
        }
    }

    internal class IdPreprocessor
    {
        private int _nextId;
        private List<IPoint> _points;

        private void Process(IPoint point)
        {
            // add only unique points
            if (point.Id == 0)
            {
                point.Id = NextId();
                _points.Add(point);
            }
        }

        private void Process(IPin pin)
        {
            pin.Id = NextId();
            Process(pin.Point);
            Process(pin.Shape);
        }

        private void Process(ILine line)
        {
            line.Id = NextId();
            Process(line.Point1);
            Process(line.Point2);
        }

        private void Process(IBezier bezier)
        {
            bezier.Id = NextId();
            Process(bezier.Start);
            Process(bezier.Point1);
            Process(bezier.Point2);
            Process(bezier.Point3);
        }

        private void Process(IQuadraticBezier quadraticBezier)
        {
            quadraticBezier.Id = NextId();
            Process(quadraticBezier.Start);
            Process(quadraticBezier.Point1);
            Process(quadraticBezier.Point2);
        }

        private void Process(IArc arc)
        {
            arc.Id = NextId();
            Process(arc.Point1);
            Process(arc.Point2);
        }

        private void Process(IRectangle rectangle)
        {
            rectangle.Id = NextId();
            Process(rectangle.Point1);
            Process(rectangle.Point2);
        }

        private void Process(IEllipse ellipse)
        {
            ellipse.Id = NextId();
            Process(ellipse.Point1);
            Process(ellipse.Point2);
        }

        private void Process(IText text)
        {
            text.Id = NextId();
            Process(text.Point1);
            Process(text.Point2);
        }

        private void Process(IList<INative> children)
        {
            int count = children.Count;
            for (int i = 0; i < count; i++)
            {
                Process(children[i]);
            }
        }

        private void Process(INative child)
        {
            if (child is IPin)
            {
                Process(child as IPin);
            }
            else if (child is ILine)
            {
                Process(child as ILine);
            }
            else if (child is IBezier)
            {
                Process(child as IBezier);
            }
            else if (child is IQuadraticBezier)
            {
                Process(child as IQuadraticBezier);
            }
            else if (child is IArc)
            {
                Process(child as IArc);
            }
            else if (child is IRectangle)
            {
                Process(child as IRectangle);
            }
            else if (child is IEllipse)
            {
                Process(child as IEllipse);
            }
            else if (child is IText)
            {
                Process(child as IText);
            }
            else if (child is IBlock)
            {
                Process(child as IBlock);
            }
        }

        private void Process(IBlock block)
        {
            block.Id = NextId();
            Process(block.Children);
        }

        private int NextId()
        {
            return _nextId++;
        }

        public BPoint[] Process(ICanvas canvas)
        {
            _nextId = 1;
            _points = new List<IPoint>();

            canvas.Id = NextId();
            Process(canvas.Children);

            var bpoints = new BPoint[_points.Count];

            for (int i = 0; i < _points.Count; i++)
            {
                var point = _points[i];
                var bpoint = new BPoint();

                bpoint.Id = point.Id;
                bpoint.Connected = new int[point.Connected.Count];

                for (int j = 0; j < point.Connected.Count; j++)
                {
                    bpoint.Connected[j] = point.Connected[j].Id;
                }

                bpoint.Point = point;
                bpoints[i] = bpoint;
            }

            return bpoints;
        }
    }

    internal class CanvasReader
    {
        private BinaryReader _reader;
        private IDictionary<int, BPoint> _bpoints;
        private IDictionary<int, INative> _natives;

        private NativeType ReadNativeType()
        {
            return (NativeType)_reader.ReadByte();
        }

        private INative ReadNative()
        {
            var type = ReadNativeType();
            switch (type)
            {
                case NativeType.Pin:
                    return ReadPin();
                case NativeType.Line:
                    return ReadLine();
                case NativeType.Bezier:
                    return ReadBezier();
                case NativeType.QuadraticBezier:
                    return ReadQuadraticBezier();
                case NativeType.Arc:
                    return ReadArc();
                case NativeType.Rectangle:
                    return ReadRectangle();
                case NativeType.Ellipse:
                    return ReadEllipse();
                case NativeType.Text:
                    return ReadText();
                case NativeType.Block:
                    return ReadBlock();
                default:
                    throw new InvalidDataException();
            }
        }

        private IPoint ReadPoint()
        {
            int id = _reader.ReadInt32();
            BPoint bpoint;
            if (_bpoints.TryGetValue(id, out bpoint))
            {
                return bpoint.Point;
            }
            throw new InvalidDataException();
        }

        private IColor ReadColor()
        {
            return new XColor(
                _reader.ReadByte(),
                _reader.ReadByte(),
                _reader.ReadByte(),
                _reader.ReadByte());
        }

        private IPin ReadPin()
        {
            var pin = new XPin()
            {
                Id = _reader.ReadInt32(),
                Point = ReadPoint(),
                Shape = ReadNative(),
            };
            _natives.Add(pin.Id, pin);
            return pin;
        }

        private ILine ReadLine()
        {
            var line = new XLine()
            {
                Id = _reader.ReadInt32(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble()  
            };
            _natives.Add(line.Id, line);
            return line;
        }

        private IBezier ReadBezier()
        {
            var bezier = new XBezier()
            {
                Id = _reader.ReadInt32(),
                Start = ReadPoint(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                Point3 = ReadPoint(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble(),
                Fill = ReadColor(),
                IsFilled = _reader.ReadBoolean(),
                IsClosed = _reader.ReadBoolean()
            };
            _natives.Add(bezier.Id, bezier);
            return bezier;
        }

        private IQuadraticBezier ReadQuadraticBezier()
        {
            var quadraticBezier = new XQuadraticBezier()
            {
                Id = _reader.ReadInt32(),
                Start = ReadPoint(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble(),
                Fill = ReadColor(),
                IsFilled = _reader.ReadBoolean(),
                IsClosed = _reader.ReadBoolean()
            };
            _natives.Add(quadraticBezier.Id, quadraticBezier);
            return quadraticBezier;
        }

        private IArc ReadArc()
        {
            var arc = new XArc()
            {
                Id = _reader.ReadInt32(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                StartAngle = _reader.ReadDouble(),
                SweepAngle = _reader.ReadDouble(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble(),
                Fill = ReadColor(),
                IsFilled = _reader.ReadBoolean(),
                IsClosed = _reader.ReadBoolean()
            };
            _natives.Add(arc.Id, arc);
            return arc;
        }

        private IRectangle ReadRectangle()
        {
            var rectangle = new XRectangle()
            {
                Id = _reader.ReadInt32(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble(),
                Fill = ReadColor()
            };
            _natives.Add(rectangle.Id, rectangle);
            return rectangle;
        }

        private IEllipse ReadEllipse()
        {
            var ellipse = new XEllipse()
            {
                Id = _reader.ReadInt32(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                Stroke = ReadColor(),
                StrokeThickness = _reader.ReadDouble(),
                Fill = ReadColor()
            };
            _natives.Add(ellipse.Id, ellipse);
            return ellipse;
        }

        private IText ReadText()
        {
            var text = new XText()
            {
                Id = _reader.ReadInt32(),
                Point1 = ReadPoint(),
                Point2 = ReadPoint(),
                HorizontalAlignment = _reader.ReadInt32(),
                VerticalAlignment = _reader.ReadInt32(),
                Size = _reader.ReadDouble(),
                Text = _reader.ReadString(),
                Foreground = ReadColor(),
                Backgroud = ReadColor()
            };
            _natives.Add(text.Id, text);
            return text;
        }

        private IBlock ReadBlock()
        {
            var block = new XBlock()
            {
                Id = _reader.ReadInt32()
            };
            var children = block.Children;

            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                var type = ReadNativeType();
                switch (type)
                {
                    case NativeType.Pin:
                        children.Add(ReadPin());
                        break;
                    case NativeType.Line:
                        children.Add(ReadLine());
                        break;
                    case NativeType.Bezier:
                        children.Add(ReadBezier());
                        break;
                    case NativeType.QuadraticBezier:
                        children.Add(ReadQuadraticBezier());
                        break;
                    case NativeType.Arc:
                        children.Add(ReadArc());
                        break;
                    case NativeType.Rectangle:
                        children.Add(ReadRectangle());
                        break;
                    case NativeType.Ellipse:
                        children.Add(ReadEllipse());
                        break;
                    case NativeType.Text:
                        children.Add(ReadText());
                        break;
                    case NativeType.Block:
                        children.Add(ReadBlock());
                        break;
                    case NativeType.End:
                        _natives.Add(block.Id, block);
                        return block;
                    default:
                        throw new InvalidDataException();
                }
            }

            throw new InvalidDataException();
        }

        private BPoint ReadBPoint()
        {
            int id = _reader.ReadInt32();
            int length = _reader.ReadInt32();

            var bpoint = new BPoint()
            {
                Id = id
            };

            bpoint.Connected = new int[length];
            for (int i = 0; i < length; i++)
            {
                int connected = _reader.ReadInt32();
                bpoint.Connected[i] = connected;
            }

            double x = _reader.ReadDouble();
            double y = _reader.ReadDouble();
            var point = new XPoint(x, y)
            {
                Id = id
            };

            bpoint.Point = point;
            return bpoint;
        }

        public void UpdatePointConnections()
        {
            foreach (var bpoint in _bpoints)
            {
                for (int i = 0; i < bpoint.Value.Connected.Length; i++)
                {
                    INative native;
                    if (_natives.TryGetValue(bpoint.Value.Connected[i], out native))
                    {
                        bpoint.Value.Point.Connected.Add(native);
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
            }
        }

        public ICanvas Read(BinaryReader reader)
        {
            _reader = reader;
            _natives = new Dictionary<int, INative>();

            // read binary points
            _bpoints = new Dictionary<int, BPoint>();
            int length = _reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var bpoint = ReadBPoint();
                _bpoints.Add(bpoint.Id, bpoint);
            }

            // read canvas contents
            var nativeType = ReadNativeType();
            if (nativeType != NativeType.Canvas)
            {
                throw new InvalidDataException();
            }

            var canvas = new XCanvas()
            {
                Id = _reader.ReadInt32(),
                Width = _reader.ReadDouble(),
                Height = _reader.ReadDouble(),
                Background = ReadColor(),
                EnableSnap = _reader.ReadBoolean(),
                SnapX = _reader.ReadDouble(),
                SnapY = _reader.ReadDouble()
            };
            var children = canvas.Children;

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var type = ReadNativeType();
                switch (type)
                {
                    case NativeType.Pin:
                        children.Add(ReadPin());
                        break;
                    case NativeType.Line:
                        children.Add(ReadLine());
                        break;
                    case NativeType.Bezier:
                        children.Add(ReadBezier());
                        break;
                    case NativeType.QuadraticBezier:
                        children.Add(ReadQuadraticBezier());
                        break;
                    case NativeType.Arc:
                        children.Add(ReadArc());
                        break;
                    case NativeType.Rectangle:
                        children.Add(ReadRectangle());
                        break;
                    case NativeType.Ellipse:
                        children.Add(ReadEllipse());
                        break;
                    case NativeType.Text:
                        children.Add(ReadText());
                        break;
                    case NativeType.Block:
                        children.Add(ReadBlock());
                        break;
                    case NativeType.End:
                        _reader = null;
                        UpdatePointConnections();
                        return canvas;
                    default:
                        _reader = null;
                        throw new InvalidDataException();
                }
            }
            _reader = null;
            throw new InvalidDataException();
        }
    }

    internal class CanvasWriter
    {
        private BinaryWriter _writer;

        private void Write(NativeType type)
        {
            _writer.Write((byte)type);
        }

        private void Write(IPoint point)
        {
            // point are written only as Id
            _writer.Write(point.Id);
        }

        private void Write(IColor color)
        {
            _writer.Write(color.A);
            _writer.Write(color.R);
            _writer.Write(color.G);
            _writer.Write(color.B);
        }

        private void Write(IPin pin)
        {
            Write(NativeType.Pin);
            _writer.Write(pin.Id);
            Write(pin.Point);
            Write(pin.Shape);
        }

        private void Write(ILine line)
        {
            Write(NativeType.Line);
            _writer.Write(line.Id);
            Write(line.Point1);
            Write(line.Point2);
            Write(line.Stroke);
            _writer.Write(line.StrokeThickness);
        }

        private void Write(IBezier bezier)
        {
            Write(NativeType.Bezier);
            _writer.Write(bezier.Id);
            Write(bezier.Start);
            Write(bezier.Point1);
            Write(bezier.Point2);
            Write(bezier.Point3);
            Write(bezier.Stroke);
            _writer.Write(bezier.StrokeThickness);
            Write(bezier.Fill);
            _writer.Write(bezier.IsFilled);
            _writer.Write(bezier.IsClosed);
        }

        private void Write(IQuadraticBezier quadraticBezier)
        {
            Write(NativeType.QuadraticBezier);
            _writer.Write(quadraticBezier.Id);
            Write(quadraticBezier.Start);
            Write(quadraticBezier.Point1);
            Write(quadraticBezier.Point2);
            Write(quadraticBezier.Stroke);
            _writer.Write(quadraticBezier.StrokeThickness);
            Write(quadraticBezier.Fill);
            _writer.Write(quadraticBezier.IsFilled);
            _writer.Write(quadraticBezier.IsClosed);
        }

        private void Write(IArc arc)
        {
            Write(NativeType.Arc);
            _writer.Write(arc.Id);
            Write(arc.Point1);
            Write(arc.Point2);
            _writer.Write(arc.StartAngle);
            _writer.Write(arc.SweepAngle);
            Write(arc.Stroke);
            _writer.Write(arc.StrokeThickness);
            Write(arc.Fill);
            _writer.Write(arc.IsFilled);
            _writer.Write(arc.IsClosed);
        }

        private void Write(IRectangle rectangle)
        {
            Write(NativeType.Rectangle);
            _writer.Write(rectangle.Id);
            Write(rectangle.Point1);
            Write(rectangle.Point2);
            Write(rectangle.Stroke);
            _writer.Write(rectangle.StrokeThickness);
            Write(rectangle.Fill);
        }

        private void Write(IEllipse ellipse)
        {
            Write(NativeType.Ellipse);
            _writer.Write(ellipse.Id);
            Write(ellipse.Point1);
            Write(ellipse.Point2);
            Write(ellipse.Stroke);
            _writer.Write(ellipse.StrokeThickness);
            Write(ellipse.Fill);
        }

        private void Write(IText text)
        {
            Write(NativeType.Text);
            _writer.Write(text.Id);
            Write(text.Point1);
            Write(text.Point2);
            _writer.Write(text.HorizontalAlignment);
            _writer.Write(text.VerticalAlignment);
            _writer.Write(text.Size);
            _writer.Write(text.Text);
            Write(text.Foreground);
            Write(text.Backgroud);
        }

        private void Write(IList<INative> children)
        {
            int count = children.Count;
            for (int i = 0; i < count; i++)
            {
                Write(children[i]);
            }
        }

        private void Write(INative child)
        {
            if (child is IPin)
            {
                Write(child as IPin);
            }
            else if (child is ILine)
            {
                Write(child as ILine);
            }
            else if (child is IBezier)
            {
                Write(child as IBezier);
            }
            else if (child is IQuadraticBezier)
            {
                Write(child as IQuadraticBezier);
            }
            else if (child is IArc)
            {
                Write(child as IArc);
            }
            else if (child is IRectangle)
            {
                Write(child as IRectangle);
            }
            else if (child is IEllipse)
            {
                Write(child as IEllipse);
            }
            else if (child is IText)
            {
                Write(child as IText);
            }
            else if (child is IBlock)
            {
                Write(child as IBlock);
            }
        }

        private void Write(IBlock block)
        {
            Write(NativeType.Block);
            _writer.Write(block.Id);
            Write(block.Children);
            Write(NativeType.End);
        }

        private void Write(ICanvas canvas)
        {
            Write(NativeType.Canvas);
            _writer.Write(canvas.Id);
            _writer.Write(canvas.Width);
            _writer.Write(canvas.Height);
            Write(canvas.Background);
            _writer.Write(canvas.EnableSnap);
            _writer.Write(canvas.SnapX);
            _writer.Write(canvas.SnapY);
            Write(canvas.Children);
            Write(NativeType.End);
        }

        private void Write(ref BPoint bpoint)
        {
            _writer.Write(bpoint.Id);
            _writer.Write(bpoint.Connected.Length);
            for (int i = 0; i < bpoint.Connected.Length; i++)
            {
                Debug.Assert(bpoint.Connected[i] != 0);
                _writer.Write(bpoint.Connected[i]);
            }
            _writer.Write(bpoint.Point.X);
            _writer.Write(bpoint.Point.Y);
        }

        private void Write(ref BPoint[] bpoints)
        {
            _writer.Write(bpoints.Length);
            for (int i = 0; i < bpoints.Length; i++)
            {
                Write(ref bpoints[i]);
            }
        }

        public void Write(BinaryWriter writer, ref BPoint[] bpoints, ICanvas canvas)
        {
            _writer = writer;

            Write(ref bpoints);
            Write(canvas);

            _writer = null;
        }
    }

    public class BinaryFile : IFile
    {
        public string Name { get; set; }
        public string Extension { get; set; }

        public BinaryFile()
        {
            Name = "Binary";
            Extension = "bin";
        }

        public ICanvas Open(string path)
        {
            return null;
            /*
            using (var file = File.Open(path, FileMode.Open))
            {
                return Read(file);
            }
            */
        }

        public void Save(string path, ICanvas canvas)
        {
            /*
            using (var file = File.Create(path))
            {
                Write(file, canvas);
            }
            */
        }

        public ICanvas Read(Stream stream)
        {
            var idReset = new IdReset();
            var canvasReader = new CanvasReader();

            using (var reader = new BinaryReader(stream))
            {
                var canvas = canvasReader.Read(reader);
                idReset.Reset(canvas);
                return canvas;
            }
        }

        public void Write(Stream stream, ICanvas canvas)
        {
            var idReset = new IdReset();
            idReset.Reset(canvas);

            var idPreprocessor = new IdPreprocessor();
            var bpoints = idPreprocessor.Process(canvas);
            var canvasWriter = new CanvasWriter();

            using (var writer = new BinaryWriter(stream))
            {
                canvasWriter.Write(writer, ref bpoints, canvas);
                idReset.Reset(canvas);
            }
        }
    }
}
