using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CanvasDiagram.Droid
{
    public static class Editor
    {
        public const int StandalonePinId = -1;

        public const char ModelSeparator = ';';
        public const string TagPin = "Pin";
        public const string TagWire = "Wire";
        public const string TagAndGate = "AndGate";
        public const string TagOrGate = "OrGate";
        public const char ModelNewLine = '\n';

        private static char[] ModelArgSeparators = new char[] { ';', '\t', ' ' };
        private static char[] ModelLineSeparators = new char[] { ModelNewLine };

        public static void UpdateWireConnections(ConcurrentDictionary<int, Element> elements)
        {
            var wires = elements.Where(x => x.Value is Wire);

            foreach (var pair in wires)
                {
                    var wire = pair.Value as Wire;

                    if (wire.StartParentId == StandalonePinId)
                        {
                            Element start;
                            if (elements.TryGetValue(wire.StartId, out start))
                                {
                                    var pin = start as Pin;
                                    wire.Start = pin;
                                    pin.Wires.Add(wire);
                                }
                        }
                    else
                        {
                            Element parent;
                            if (elements.TryGetValue(wire.StartParentId, out parent))
                                {
                                    var pin = parent.Pins.FirstOrDefault(x => x.Id == wire.StartId);
                                    wire.Start = pin;
                                    pin.Wires.Add(wire);
                                }
                        }

                    if (wire.EndParentId == StandalonePinId)
                        {
                            Element end;
                            if (elements.TryGetValue(wire.EndId, out end))
                                {
                                    var pin = end as Pin;
                                    wire.End = pin;
                                    pin.Wires.Add(wire);
                                }
                        }
                    else
                        {
                            Element parent;
                            if (elements.TryGetValue(wire.EndParentId, out parent))
                                {
                                    var pin = parent.Pins.FirstOrDefault(x => x.Id == wire.EndId);
                                    wire.End = pin;
                                    pin.Wires.Add(wire);
                                }
                        }

                    wire.Initialize(4f, 3f);
                }
        }

        private static StringBuilder sb = new StringBuilder();

        public static string Serialize(ICollection<KeyValuePair<int, Element>> elements)
        {
            //var sb = new StringBuilder(); 
            sb.Length = 0;

            //var sw = System.Diagnostics.Stopwatch.StartNew ();

            foreach (var pair in elements)
                {
                    var element = pair.Value;

                    if (element is Pin)
                        {
                            var pin = element as Pin;
                            //string str = string.Format ("Pin;{0};{1};{2}", pin.Id, pin.X, pin.Y);
                            //sb.AppendLine (str);

                            sb.Append(TagPin); 
                            sb.Append(ModelSeparator);

                            sb.Append(pin.Id);
                            sb.Append(ModelSeparator);

                            sb.Append(pin.X);
                            sb.Append(ModelSeparator);

                            sb.Append(pin.Y);
                            sb.Append(ModelNewLine);
                        }
                    else if (element is Wire)
                        {
                            var wire = element as Wire;
                            //string str = string.Format ("Wire;{0};{1};{2};{3};{4}", 
                            //                            wire.Id, 
                            //                            wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId, 
                            //                            wire.Start.Id, 
                            //                            wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId, 
                            //                            wire.End.Id);
                            //sb.AppendLine (str);

                            sb.Append(TagWire); 
                            sb.Append(ModelSeparator);

                            sb.Append(wire.Id);
                            sb.Append(ModelSeparator);

                            sb.Append(wire.Start.Parent != null ? wire.Start.Parent.Id : StandalonePinId);
                            sb.Append(ModelSeparator);

                            sb.Append(wire.Start.Id);
                            sb.Append(ModelSeparator);

                            sb.Append(wire.End.Parent != null ? wire.End.Parent.Id : StandalonePinId);
                            sb.Append(ModelSeparator);

                            sb.Append(wire.End.Id);
                            sb.Append(ModelNewLine);
                        }
                    else if (element is AndGate)
                        {
                            var andGate = element as AndGate;
                            //string str = string.Format ("AndGate;{0};{1};{2}", andGate.Id, andGate.X, andGate.Y);
                            //sb.AppendLine (str);

                            sb.Append(TagAndGate); 
                            sb.Append(ModelSeparator);

                            sb.Append(andGate.Id);
                            sb.Append(ModelSeparator);

                            sb.Append(andGate.X);
                            sb.Append(ModelSeparator);

                            sb.Append(andGate.Y);
                            sb.Append(ModelNewLine);
                        }
                    else if (element is OrGate)
                        {
                            var orGate = element as OrGate;
                            //string str = string.Format ("OrGate;{0};{1};{2}", orGate.Id, orGate.X, orGate.Y);
                            //sb.AppendLine (str);

                            sb.Append(TagOrGate); 
                            sb.Append(ModelSeparator);

                            sb.Append(orGate.Id);
                            sb.Append(ModelSeparator);

                            sb.Append(orGate.X);
                            sb.Append(ModelSeparator);

                            sb.Append(orGate.Y);
                            sb.Append(ModelNewLine);
                        }
                }

            //sw.Stop ();
            //Console.WriteLine ("Generate: {0}ms", sw.Elapsed.TotalMilliseconds);

            return sb.ToString();
        }

        public static void Deserialize(string model, ConcurrentDictionary<int, Element> elements)
        {
            if (string.IsNullOrEmpty(model))
            {
                return;
            }

            string type = null;
            var lines = model.Split(ModelLineSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var element in lines)
            {
                var args = element.Split(ModelArgSeparators, StringSplitOptions.RemoveEmptyEntries);

                int length = args.Length;
                if (length < 2)
                    continue;

                type = args[0];

                if (string.Compare(type, "Pin", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var pin = new Pin(id, null, x, y, 4f, 3f);
                    elements.TryAdd(id, pin);
                }
                else if (string.Compare(type, "Wire", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 6)
                {
                    int id = int.Parse(args[1]);
                    int startParentId = int.Parse(args[2]);
                    int startId = int.Parse(args[3]);
                    int endParentId = int.Parse(args[4]);
                    int endtId = int.Parse(args[5]);

                    var wire = new Wire(id, startParentId, startId, endParentId, endtId);
                    elements.TryAdd(id, wire);
                }
                else if (string.Compare(type, "AndGate", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var andGate = new AndGate(id, x, y);
                    elements.TryAdd(id, andGate);
                }
                else if (string.Compare(type, "OrGate", StringComparison.InvariantCultureIgnoreCase) == 0 && length == 4)
                {
                    int id = int.Parse(args[1]);
                    float x = float.Parse(args[2]);
                    float y = float.Parse(args[3]);

                    var orGate = new OrGate(id, x, y, 1);
                    elements.TryAdd(id, orGate);
                }
            }

            UpdateWireConnections(elements);
        }
    }
}
