using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Destination;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Task.Generic;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Destination
{
    public class XmlDestination : WorkTask, IXmlDestination
    {
        private string _destFile;
        private string _workingFilename;
        private object _returnObject;
        private bool _useLoopValueAsFilename;
        private XMLDestinationType _destinationType;
        private IEnumerable<IRow> _arr = null;
        private List<Tuple<string, string, string, string>> _namespaces;
        private XmlDestinationInfo _rootNode;
        private XmlDestinationInfo _node;
        private XmlDestinationInfo _newRow;

        const string xsi = "http://www.w3.org/2001/XMLSchema-instance";


        public XmlDestination(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
            _namespaces = new List<Tuple<string, string, string, string>>();
            _rootNode = new XmlDestinationInfo();
            _rootNode.Children = new List<XmlDestinationInfo>();
            _rootNode.Rows = new List<XmlDestinationInfo>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            base.Initialize(taskResult);
            Trace("Start Initialize XmlDestination", taskResult);
            if (TaskResult is LoopTaskResult)
            {
                _arr = (IEnumerable<IRow>)((LoopTaskResult)TaskResult).Result;
                if (_useLoopValueAsFilename)
                {
                    _destFile = ((LoopTaskResult)TaskResult).LoopValue.ToString();
                    _workingFilename = _destFile;
                    _destinationType = XMLDestinationType.Filename;
                }
                else
                {
                    if (_destinationType == XMLDestinationType.Filename && !String.IsNullOrEmpty(_destFile))
                    {
                        var directory = Path.GetDirectoryName(_destFile);
                        var fileWithoutExtension = Path.GetFileNameWithoutExtension(_destFile);
                        var extension = Path.GetExtension(_destFile);
                        _workingFilename = Path.Combine(directory, String.Concat(fileWithoutExtension, $"_{Guid.NewGuid().ToString("N").Substring(0, 8)}", extension));
                    }
                }
            }
            else
            {
                _arr = (IEnumerable<IRow>)TaskResult.Result;
            }
        }

        public IXmlDestination ToFile(string file)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException("File is null");

            Trace("Set ToFile", file);

            this._destFile = file;
            _workingFilename = file;
            _destinationType = XMLDestinationType.Filename;
            return this;
        }

        public IXmlDestination UseLoopValueAsFilename()
        {
            Trace("Set UseLoopValueAsFilename");
            _useLoopValueAsFilename = true;
            return this;
        }

        public IXmlDestination Element(Func<string> nodeElement)
        {
            if (nodeElement == null)
                throw new ArgumentNullException("Node Element is null or empty");

            _node = new XmlDestinationInfo();
            _node.Children = new List<XmlDestinationInfo>();
            _node.Rows = new List<XmlDestinationInfo>();
            _node.NodeName = nodeElement();
            _node.IsElement = true;

            Trace("Set Element", _node);
            return this;
        }

        public IXmlDestination ToXmlString()
        {
            Trace("Set ToXmlString");
            _destinationType = XMLDestinationType.String;
            return this;
        }

        public IXmlDestination ToStream()
        {
            Trace("Set ToStream");
            _destinationType = XMLDestinationType.Stream;
            return this;
        }

        public IXmlDestinationNoteType Use(Func<string> node)
        {
            Use(node, null);
            return this;
        }

        public IXmlDestinationNoteType Use(Func<string> node, Func<IFormatProvider> provider)
        {
            if (node == null)
                throw new ArgumentNullException("NodeName is null or empty");

            _node = new XmlDestinationInfo();
            _node.NodeName = node();
            _node.Children = new List<XmlDestinationInfo>();
            _node.Rows = new List<XmlDestinationInfo>();

            if (provider != null)
                _node.FormatProvider = provider();


            Trace("Set Use", _node);
            return this;
        }

        public IXmlDestination Root(string root)
        {
            if (String.IsNullOrEmpty(root))
                throw new ArgumentNullException("Root is null");

            this._rootNode.NodeName = root;

            Trace("Set Root", _rootNode);
            return this;
        }

        public IXmlDestination AddNameSpace(string prefix, string localName, string ns, string value)
        {
            if (String.IsNullOrEmpty(prefix))
                throw new ArgumentNullException("prefix is null or empty");

            if (String.IsNullOrEmpty(localName))
                throw new ArgumentNullException("localName is null or empty");

            if (String.IsNullOrEmpty(ns))
                throw new ArgumentNullException("namespace is null or empty");

            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException("value is null or empty");

            this._namespaces.Add(new Tuple<string, string, string, string>(prefix, localName, ns, value));
            Trace("Set Namespace", _namespaces);
            return this;
        }

        public IXmlDestination Parent(Func<string> parentNode)
        {
            string parent = parentNode();
            if (String.IsNullOrEmpty(parent))
                throw new ArgumentNullException("parentNode Name is null or empty");

            if (parent == _rootNode.NodeName)
            {
                _node.Parent = _rootNode;
                _rootNode.Children.Add(_node);
            }
            else
            {
                CalculateNodes(_rootNode.Children, parent);
            }

            Trace("Set Parent", _rootNode);

            return this;
        }

        private void CalculateNodes(List<XmlDestinationInfo> nodes, string parentNodeName)
        {
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var item in nodes)
                {
                    if (parentNodeName == item.NodeName)
                    {
                        _node.Parent = item;
                        item.Children.Add(_node);
                    }
                    else
                    {
                        CalculateNodes(item.Children, parentNodeName);
                    }
                }
            }
        }

        public IXmlDestinationNode IsAttribute()
        {
            if (_node == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _node.XmlNodeType = XMLNodeType.IsAttribute;
            Trace("Set IsAttribute");
            return this;
        }


        public IXmlDestinationNode IsNode()
        {
            if (_node == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _node.XmlNodeType = XMLNodeType.IsNode;
            Trace("Set IsNode");
            return this;
        }

        public IXmlDestinationUseNodeRow Field(Func<string> node)
        {
            Field(node, null);
            return this;
        }

        public IXmlDestinationUseNodeRow Field(Func<string> node, Func<IFormatProvider> provider)
        {
            if (node == null)
                throw new ArgumentNullException("NodeName is null or empty");

            _newRow = new XmlDestinationInfo();
            _newRow.NodeName = node();
            _newRow.Children = new List<XmlDestinationInfo>();
            _newRow.Rows = new List<XmlDestinationInfo>();

            if (provider != null)
                _newRow.FormatProvider = provider();

            Trace("Set Field", _newRow);
            return this;
        }

        public IXmlDestinationRow IsAttributeField()
        {
            if (_newRow == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _newRow.XmlNodeType = XMLNodeType.IsAttribute;
            if (!_node.Rows.Contains(_newRow))
                _node.Rows.Add(_newRow);

            Trace("Set IsAttributeField", _newRow);
            return this;
        }


        public IXmlDestinationRow IsNodeField()
        {
            if (_newRow == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _newRow.XmlNodeType = XMLNodeType.IsNode;

            if (!_node.Rows.Contains(_newRow))
                _node.Rows.Add(_newRow);
            Trace("Set IsNodeField", _newRow);
            return this;
        }

        public IXmlDestination CreateRow(Func<IXmlDestinationRow, IXmlDestinationRow> row)
        {
            if (row == null)
                throw new ArgumentNullException("Row is null or empty");

            row(this);

            Trace("Set CreateRow");
            return this;
        }

        public IXmlDestinationRow Add(Func<IXmlDestinationUseRow, IXmlDestinationRow> addRow)
        {
            if (addRow == null)
                throw new ArgumentNullException("Row is null or empty");

            if (_node == null)
                throw new ArgumentNullException("Identify row is null or empty");

            addRow(this);
            Trace("Set Add");
            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;
            try
            {
                if (_arr != null && _arr.Any())
                {
                    Trace($"Process Xml destination. Row count: {_arr.Count()}");

                    if (String.IsNullOrEmpty(_rootNode.NodeName))
                        throw new ArgumentNullException("Root node is empty");

                    using (XmlWriter writer = Create())
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement(_rootNode.NodeName);

                        if (_namespaces != null && _namespaces.Any())
                        {
                            foreach (var ns in _namespaces)
                            {
                                Trace("Processing namespace", ns);
                                writer.WriteAttributeString(ns.Item1, ns.Item2, ns.Item3, ns.Item4);
                            }
                        }

                        if (_rootNode.Children != null && _rootNode.Children.Count > 0)
                        {
                            foreach (var child in _rootNode.Children)
                            {
                                Trace("Processing children", child);
                                WriteChildElement(child, writer, _arr.ToList());
                            }
                        }

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
                result = new TaskResult(true, _returnObject);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }

        private void WriteRowElement(XmlDestinationInfo item, XmlWriter writer, IRow row)
        {
            Trace("Write Row element", new { XmlDestinationInfo = item, Row = row } );

            if (row.ContainsField(item.NodeName))
            {
                var value = row[item.NodeName];

                if (item.XmlNodeType == XMLNodeType.IsNode)
                {
                    writer.WriteElementString(item.NodeName,
                        item.FormatProvider != null
                            ? Convert.ToString(value, item.FormatProvider)
                            : value.ToString());
                }

                if (item.XmlNodeType == XMLNodeType.IsAttribute)
                {
                    writer.WriteAttributeString(item.NodeName,
                        item.FormatProvider != null
                            ? Convert.ToString(value, item.FormatProvider)
                            : value.ToString());
                }
            }
        }

        private void WriteChildElement(XmlDestinationInfo item, XmlWriter writer, List<IRow> rows)
        {
            Trace("Write Child element", new { XmlDestinationInfo = item, Rows = rows });

            var firstRow = rows.FirstOrDefault();
            if (item.IsElement && item.Rows.Count == 0)
            {
                writer.WriteStartElement(item.NodeName);
            }

            else if (firstRow.ContainsField(item.NodeName))
            {
                var value = firstRow[item.NodeName];

                if (item.XmlNodeType == XMLNodeType.IsNode)
                {
                    writer.WriteElementString(item.NodeName,
                        item.FormatProvider != null
                            ? Convert.ToString(value, item.FormatProvider)
                            : value.ToString());
                }

                if (item.XmlNodeType == XMLNodeType.IsAttribute)
                {
                    writer.WriteAttributeString(item.NodeName,
                        item.FormatProvider != null
                            ? Convert.ToString(value, item.FormatProvider)
                            : value.ToString());
                }

            }

            if (item.Children != null && item.Children.Count > 0)
            {
                foreach (var child in item.Children)
                {
                    WriteChildElement(child, writer, rows);
                }
            }

            if (item.Rows != null && item.Rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    writer.WriteStartElement(item.NodeName);

                    foreach (var itemRow in item.Rows)
                    {
                        WriteRowElement(itemRow, writer, row);
                    }
                    writer.WriteEndElement();

                }
            }
            if (item.IsElement && item.Rows.Count == 0)
            {
                writer.WriteEndElement();
            }
        }

        private XmlWriter Create()
        {
            XmlWriter src = null;
            Trace("Create with Destination Type", _destinationType);
            switch (_destinationType)
            {
                case XMLDestinationType.Filename:
                    {
                        if (String.IsNullOrEmpty(_workingFilename))
                            throw new ArgumentNullException("Destination File is empty");
                        _returnObject = _workingFilename;
                        src = XmlWriter.Create(_workingFilename, CraeteParserSettings());
                    }
                    break;
                case XMLDestinationType.String:
                    {
                        var sb = new StringBuilder();
                        _returnObject = sb;
                        src = XmlWriter.Create(sb, CraeteParserSettings());
                    }
                    break;
                case XMLDestinationType.Stream:
                    {
                        var stream = new MemoryStream();
                        _returnObject = stream;
                        src = XmlWriter.Create(stream, CraeteParserSettings());
                    }
                    break;
            }

            return src;
        }

        private XmlWriterSettings CraeteParserSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            return settings;
        }      
    }
}
