﻿using OData.Schema.Validation.Utils.XmlComparer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GillSoft.XmlComparer
{
    public class Comparer : IDisposable
    {
        public List<ElementAddedEventArgs> Additions = new List<ElementAddedEventArgs>();
        public List<ElementChangedEventArgs> Changes = new List<ElementChangedEventArgs>();
        public List<ElementRemovedEventArgs> Removals = new List<ElementRemovedEventArgs>();
        public IXmlCompareHandler Callback;

        public Comparer(IXmlCompareHandler callback)
        {
            Callback = callback;
        }

        private class Elem
        {
            public int LineNumber { get; private set; }

            public XElement Element { get; private set; }

            public KeyValueElementInfo KeyValueInfo { get; private set; }

            public int Source { get; private set; }

            public string XPath { get; private set; }

            public static Elem Create(XElement element, int source)
            {
                var kv = element.GetBestKeyValueInfo();
                var res = new Elem
                {
                    LineNumber = element.LineNumber(),
                    Element = element,
                    KeyValueInfo = kv,
                    Source = source,
                    XPath = element.GetXPath(kv),
                };
                return res;
            }

            public override string ToString()
            {
                var res = string.Format("{0,-5}: {1} - {2}", LineNumber, Source, Element.Name.LocalName, XPath);
                return res;
            }
        }

        public void PrintResults()
        {
            foreach (var item in Removals)
            {
                Callback.ElementRemoved(item);
            }
            foreach (var item in Additions)
            {
                Callback.ElementAdded(item);
            }
            foreach (var item in Changes)
            {
                Callback.ElementChanged(item);
            }
        }

        public ComparisonReport Compare(Stream stream1, Stream stream2)
        {
            var loadOptions = LoadOptions.SetBaseUri | LoadOptions.SetLineInfo;
            const int leftId = 1;
            const int rightId = 2;

            var doc1 = XDocument.Load(stream1, loadOptions);
            var doc2 = XDocument.Load(stream2, loadOptions);

            var nsm1 = new XmlNamespaceManagerEnhanced(doc1);
            var nsm2 = new XmlNamespaceManagerEnhanced(doc2);

            var doc1Descendants = doc1.Descendants()
                .Where(a => a != doc1.Root)
                .Select(a => new { Source = leftId, Element = a });

            var doc2Descendants = doc2.Descendants()
                .Where(a => a != doc2.Root)
                .Select(a => new { Source = rightId, Element = a });

            var doc1DescendantsDiff = doc1Descendants
                .Where(a => !doc2Descendants.Any(a2 => ElementsAreEqual(a2.Element, a.Element)))
                .Select(a => Elem.Create(a.Element, a.Source))
                .ToList();

            var doc2DescendantsDiff = doc2Descendants
                .Where(a => !doc1Descendants.Any(a1 => ElementsAreEqual(a1.Element, a.Element)))
                .Select(a => Elem.Create(a.Element, a.Source))
                .ToList();

            var itemsToProcess = doc1DescendantsDiff
                .Concat(doc2DescendantsDiff)
                .OrderBy(a => a.LineNumber).ThenBy(a => a.Source)
                .ToList();

            var xPathsAdded = new List<string>();
            var xPathsRemoved = new List<string>();
            var xPathsChanged = new List<string>();

            foreach (var item in itemsToProcess)
            {
                if (xPathsAdded.Any(a => a == item.XPath)
                    || xPathsChanged.Any(a => a == item.XPath)
                    || xPathsRemoved.Any(a => a == item.XPath)
                )
                    continue;

                //Console.WriteLine(item.XPath);

                var node1 = item.Source != leftId ? default(XElement) : item.Element;
                var node2 = item.Source != rightId ? default(XElement) : item.Element;

                // now get item from other side
                switch (item.Source)
                {
                    case leftId:
                        {
                            // get node2
                            var e = itemsToProcess.FirstOrDefault(a => a.Source == rightId && a.XPath == item.XPath);
                            if (e != null)
                            {
                                node2 = e.Element;
                            }
                            break;
                        }
                    case rightId:
                        {
                            // get node1
                            var e = itemsToProcess.FirstOrDefault(a => a.Source == leftId && a.XPath == item.XPath);
                            if (e != null)
                            {
                                node1 = e.Element;
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception("Invalid Source " + item + " for item : " + item.XPath);
                        }
                }


                if (node1 != null && node2 != null)
                {
                    CompareAttributes(node1, node2, Callback);

                    // if there are sub-elements, those will be handled separately
                    if (node1.HasElements || node2.HasElements)
                        continue;
                }

                if (node1 == null && node2 != null)
                {
                    //added
                    if (xPathsAdded.Any(a => item.XPath.StartsWith(a)))
                    {
                        // if the node's parent exists in the list of items,
                        // there is no need to call the callback
                        continue;
                    }

                    xPathsAdded.Add(item.XPath);

                    Additions.Add(new ElementAddedEventArgs(item.XPath, node2, node2.LineNumber()));
                    continue;
                }


                if (node1 != null && node2 == null)
                {
                    //removed
                    if (xPathsRemoved.Any(a => item.XPath.StartsWith(a)))
                    {
                        // if the node's parent exists in the list of items,
                        // there is no need to call the callback
                        continue;
                    }

                    xPathsRemoved.Add(item.XPath);

                    Removals.Add(new ElementRemovedEventArgs(item.XPath, node1, node1.LineNumber()));
                    continue;
                }


                if (node1 != null && node2 != null)
                {
                    //might have changed
                    //compare values

                    if (xPathsChanged.Any(a => item.XPath.StartsWith(a)))
                    {
                        // if the node's parent exists in the list of items,
                        // there is no need to call the callback
                        continue;
                    }

                    xPathsChanged.Add(item.XPath);

                    var val1 = node1.Value;
                    var val2 = node2.Value;

                    if (string.Equals(val1, val2))
                        continue;

                    Changes.Add(new ElementChangedEventArgs(item.XPath, node1, node1.LineNumber(), node2, node2.LineNumber()));
                    continue;
                }

                throw new Exception("Invalid scenario while comparing elements: " + item.XPath);
            }

            return new ComparisonReport(Additions,Changes,Removals);
        }

        private bool ElementsAreEqual(XElement xElement1, XElement xElement2)
        {
            if (xElement1 == null && xElement2 == null)
                return true;

            if (xElement1 == null || xElement2 == null)
                return false;

            if (xElement1.Name.ToString() != xElement2.Name.ToString())
                return false;

            if (!xElement1.HasAttributes != xElement2.HasAttributes)
                return false;

            if (!xElement1.HasElements != xElement2.HasElements)
                return false;

            if (!xElement1.Attributes().Any(a1 => !xElement2.Attributes().Any(a2 => a2.Value == a1.Value)))
                return false;

            return string.Equals(xElement1.Value, xElement2.Value);
        }

        public void Compare(string file1, string file2, IXmlCompareHandler callback)
        {
            using (var stream1 = File.OpenRead(file1))
            {
                using (var stream2 = File.OpenRead(file2))
                {
                    this.Compare(stream1, stream2);
                }
            }
        }

        private void CompareAttributes(XElement node1, XElement node2, IXmlCompareHandler callback)
        {
            var attribs = node1.GetAttributes()
                .Concat(node2.GetAttributes())
                .GroupBy(a => a.Name.ToString())
                .Select(a => a.First());

            foreach (var attrib in attribs)
            {
                // compare Attributes in both documents
                var attribute1 = node1.Attribute(attrib.Name);
                var attribute2 = node2.Attribute(attrib.Name);

                if (attribute1 == null && attribute2 != null)
                {
                    //added
                    callback.AttributeAdded(new AttributeAddedEventArgs(attribute2.GetXPath(), attribute2, attribute2.Parent.LineNumber()));
                    continue;
                }


                if (attribute1 != null && attribute2 == null)
                {
                    //removed
                    callback.AttributeRemoved(new AttributeRemovedEventArgs(attribute1.GetXPath(), attribute1, attribute1.Parent.LineNumber()));
                    continue;
                }


                if (attribute1 != null && attribute2 != null)
                {
                    //might have changed
                    //compare values

                    var val1 = attribute1.Value;
                    var val2 = attribute2.Value;

                    if (string.Equals(val1, val2))
                        continue;

                    callback.AttributeChanged(new AttributeChangedEventArgs(attribute1.GetXPath(), attribute1, attribute1.Parent.LineNumber(), attribute2, attribute2.Parent.LineNumber()));
                    continue;
                }

                throw new Exception("Invalid scenario while comparing Attributes: " + attrib);
            }

        }

        public void Dispose()
        {
        }
    }
}
