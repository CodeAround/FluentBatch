using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class XmlSource : WorkTask, IXmlSource
    {
        private string _inputFile;
        private string _inputRoot;
        private string _inputXmlSource;
        private LoopXmlSource _xmlSource;
        private List<Tuple<string, string>> _namespaces;
        private string _resourceFilename;

        private string _rootRows;

        private string _identifyRow;
        private XmlInfo _xmlInfo;
        private List<XmlInfo> _nodeHeader;
        private List<XmlInfo> _nodeRow;
        public XmlSource(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
            _nodeHeader = new List<XmlInfo>();
            _nodeRow = new List<XmlInfo>();
            _namespaces = new List<Tuple<string, string>>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize XmlSource", taskResult);
            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                var input = ((LoopTaskResult)TaskResult).LoopValue.ToString();
                Trace($"Set Input: {input}");
                switch (_xmlSource)
                {
                    case LoopXmlSource.Filename:
                        _inputFile = input;
                        break;
                    case LoopXmlSource.Resource:
                        _resourceFilename = input;
                        break;
                    case LoopXmlSource.Xml:
                        _inputXmlSource = input;
                        break;
                }
            }
        }

        public IXmlSource FromFile(string file)
        {
            Trace(String.IsNullOrEmpty(file) ? "Set Input file is null" : $"Set Input file : {file}");
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException("File is null");

            this._inputFile = file;
            return this;
        }

        public IXmlSource LoopBehaviour(LoopXmlSource xmlSource)
        {
            Trace($"Set XML Source: {JsonConvert.SerializeObject(xmlSource, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
            this._xmlSource = xmlSource;
            return this;
        }

        public IXmlSource FromString(string xmlSource)
        {
            Trace(String.IsNullOrEmpty(xmlSource) ? "Set String xml is null" : $"Set XML Source: {xmlSource}");
            if (String.IsNullOrEmpty(xmlSource))
                throw new ArgumentNullException("String xml is null");

            this._inputXmlSource = xmlSource;
            return this;
        }

        public IXmlSource FromResource(string resourceFilename)
        {
            Trace(String.IsNullOrEmpty(resourceFilename) ? "Set resource filename is null" : $"Set Create sql work task type. Resource file name: {resourceFilename}");
            if (String.IsNullOrEmpty(resourceFilename))
            {
                throw new ArgumentNullException("resource filename is null");
            }

            this._resourceFilename = resourceFilename;
            return this;
        }

        public IXmlSourceHeader Use(Func<string> node)
        {
            Trace(node == null ? "Set NodeName is null or empty" : $"Set NodeName: {node()}");
            if (node == null)
                throw new ArgumentNullException("NodeName is null or empty");

            _xmlInfo = new XmlInfo();
            _xmlInfo.NodeName = node();
            return this;
        }

        public IXmlSource Root(string root)
        {
            Trace(String.IsNullOrEmpty(root) ? "Set Root is null or empty" : $"Set Root: {root}");
            if (String.IsNullOrEmpty(root))
                throw new ArgumentNullException("Root is null");

            this._inputRoot = root;
            return this;
        }

        public IXmlSource AddNameSpace(string prefix, string uri)
        {
            Trace(String.IsNullOrEmpty(prefix) ? "Set Prefix is null or empty" : $"Set Prefix: {prefix}");
            if (String.IsNullOrEmpty(prefix))
                throw new ArgumentNullException("prefix is null or empty");

            Trace(String.IsNullOrEmpty(uri) ? "Set Uri is null or empty" : $"Set Uri: {uri}");
            if (String.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri is null or empty");

            this._namespaces.Add(new Tuple<string, string>(prefix, uri));
            return this;
        }

        public IXmlSource RootRow(string rootRow)
        {
            Trace(String.IsNullOrEmpty(rootRow) ? "Set rootRow is null or empty" : $"Set rootRow: {rootRow}");
            if (String.IsNullOrEmpty(rootRow))
                throw new ArgumentNullException("Root is null or empty");

            _rootRows = rootRow;
            return this;
        }

        public IXmlSource IdentifyRow(string identifyRow)
        {
            Trace(String.IsNullOrEmpty(identifyRow) ? "Set identifyRow is null or empty" : $"Set identifyRow: {identifyRow}");
            if (String.IsNullOrEmpty(identifyRow))
                throw new ArgumentNullException("Identify Row is null or empty");
            _identifyRow = identifyRow;

            return this;
        }

        public IXmlSourceNoteType AsHeader()
        {
            Trace($"Set Using node {_xmlInfo.NodeName} as header.");
            if (_xmlInfo == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _xmlInfo.XmlFormatType = XMLFormatType.Header;
            return this;
        }

        public IXmlSourceNoteType AsRow()
        {
            Trace($"Set Using node {_xmlInfo.NodeName} as row.");
            if (_xmlInfo == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _xmlInfo.XmlFormatType = XMLFormatType.Row;
            return this;
        }

        public IXmlSourceHeader XPath(Func<string> xPath)
        {
            Trace($"Set XPath: {xPath()}");
            if (_xmlInfo == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _xmlInfo.XPath = xPath();
            return this;
        }

        public IXmlSource IsAttribute()
        {
            Trace($"Set Using node {_xmlInfo.NodeName} as attribute.");
            if (_xmlInfo == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _xmlInfo.XmlNodeType = XMLNodeType.IsAttribute;
            if (_xmlInfo.XmlFormatType == XMLFormatType.Header)
                _nodeHeader.Add(_xmlInfo);
            else
            {
                _nodeRow.Add(_xmlInfo);
            }
            return this;
        }

        public IXmlSource IsNode()
        {
            Trace($"Set Using node {_xmlInfo.NodeName} as node.");
            if (_xmlInfo == null)
                throw new ArgumentNullException("Node Name is null or empty");

            _xmlInfo.XmlNodeType = XMLNodeType.IsNode;
            if (_xmlInfo.XmlFormatType == XMLFormatType.Header)
                _nodeHeader.Add(_xmlInfo);
            else
            {
                _nodeRow.Add(_xmlInfo);
            }
            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;

            try
            {
                Trace("Process xml");

                var lst = LoadXml();

                Trace("End process", lst);
                result = new TaskResult(true, lst);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }

        private IEnumerable<IRow> LoadXml()
        {
            List<IRow> lst = new List<IRow>();
            Trace("Loading XML");
            var inputXml = GetStream();

            var tree = BuildTree(inputXml);

            inputXml = GetStream();

            var rows = Process(inputXml, tree);

            return rows;
        }

        private XmlParserContext CreateParserContext()
        {
            NameTable nt = new NameTable();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            Trace("Creating parser context");
            Trace($"Namespaces." , _namespaces);
            if (_namespaces != null && _namespaces.Count > 0)
            {
                foreach (var item in _namespaces)
                {
                    nsmgr.AddNamespace(item.Item1, item.Item2);
                }
            }

            XmlParserContext ctx = new XmlParserContext(null, nsmgr, null, XmlSpace.None);
            Trace("Parser context created", ctx);
            return ctx;
        }

        private XmlReaderSettings CraeteParserSettings()
        {
            Trace("Creating parser settings");
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.DtdProcessing = DtdProcessing.Ignore;
            Trace("Parser settings created. Settings", settings);
            return settings;
        }

        private IEnumerable<IRow> Process(StreamReader reader, Dictionary<string, string> tree)
        {
            var elements = new Stack<string>();
            bool startRows = false;

            List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
            Dictionary<string, object> headers = new Dictionary<string, object>();
            Dictionary<string, object> dicRow = null;
            Trace("Start processing tree");
            using (var myReader = reader)
            {
                using (XmlReader xmlReader = XmlReader.Create(myReader, CraeteParserSettings(), CreateParserContext()))
                {
                    Trace(String.IsNullOrEmpty(this._inputRoot) ? "Input root is null or empty" : $"Reading root: {this._inputRoot}");
                    if (!String.IsNullOrEmpty(this._inputRoot))
                    {
                        xmlReader.ReadStartElement(this._inputRoot);
                    }

                    while (xmlReader.Read())
                    {
                        if (xmlReader.Name.ToUpperInvariant() == _rootRows.ToUpperInvariant() &&
                            xmlReader.NodeType != XmlNodeType.EndElement)
                            startRows = true;

                        if (!startRows)
                        {
                            if (_nodeHeader.Exists(x =>
                                x.NodeName.ToUpperInvariant() == xmlReader.Name.ToUpperInvariant() &&
                                x.XmlNodeType == XMLNodeType.IsNode))
                            {
                                if (!headers.ContainsKey(xmlReader.Name))
                                {
                                    Trace($"Adding header: Name:{xmlReader.Name}, Value:{xmlReader.ReadString()}");
                                    headers.Add(xmlReader.Name, xmlReader.ReadString().Trim());
                                }
                            }
                            else if (xmlReader.HasAttributes)
                            {
                                while (xmlReader.MoveToNextAttribute())
                                {
                                    if (_nodeHeader.Exists(x =>
                                        x.NodeName.ToUpperInvariant() == xmlReader.Name.ToUpperInvariant() &&
                                        x.XmlNodeType == XMLNodeType.IsAttribute))
                                    {
                                        if (!headers.ContainsKey(xmlReader.Name))
                                        {
                                            Trace($"Adding header: Name:{xmlReader.Name}, Value:{xmlReader.ReadString()}");
                                            headers.Add(xmlReader.Name, xmlReader.Value.Trim());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (xmlReader.Name.ToUpperInvariant() == _identifyRow.ToUpperInvariant())
                            {
                                if (xmlReader.NodeType != XmlNodeType.EndElement)
                                {
                                    dicRow = new Dictionary<string, object>();
                                    Trace(headers.Count > 0 ? $"Headers count: {headers.Count}" : "No headers found");
                                    foreach (var item in headers)
                                    {
                                        if (!dicRow.ContainsKey(item.Key))
                                            dicRow.Add(item.Key, item.Value);
                                    }

                                    if (xmlReader.HasAttributes)
                                    {
                                        while (xmlReader.MoveToNextAttribute())
                                        {
                                            Trace($"Reading Attribute: Name:{xmlReader.Name}, Value:{xmlReader.Value}");
                                            if (_nodeRow.Exists(x =>
                                                x.NodeName.ToUpperInvariant() == xmlReader.Name.ToUpperInvariant() &&
                                                x.XmlNodeType == XMLNodeType.IsAttribute))
                                            {
                                                if (!dicRow.ContainsKey(xmlReader.Name))
                                                    dicRow.Add(xmlReader.Name, xmlReader.Value.Trim());
                                            }
                                        }
                                    }
                                }
                                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                                {
                                    Trace("Reading End Element");
                                    foreach (var node in _nodeHeader.Where(x => !String.IsNullOrEmpty(x.XPath)).ToList())
                                    {
                                        if (tree.ContainsKey(node.XPath))
                                            dicRow[node.NodeName] = tree[node.XPath].Trim();
                                    }

                                    values.Add(dicRow);
                                    Trace("values", values);
                                }
                            }
                            else if (_nodeRow.Exists(x =>
                                x.NodeName.ToUpperInvariant() == xmlReader.Name.ToUpperInvariant() &&
                                x.XmlNodeType == XMLNodeType.IsNode))
                            {
                                if (!dicRow.ContainsKey(xmlReader.Name))
                                    dicRow.Add(xmlReader.Name, xmlReader.ReadString().Trim());
                                Trace("Dictionary rows", dicRow);
                            }

                        }
                    }
                }
            }

            return values.Select(x => new DictionaryRow(x)).ToList();
        }
        private Dictionary<string, string> BuildTree(TextReader reader)
        {
            Dictionary<string, string> tree = new Dictionary<string, string>();
            Stack<string> elements = new Stack<string>();

            Trace("BuildTree");

            using (var myReader = reader)
            {
                using (XmlReader xmlReader = XmlReader.Create(myReader, CraeteParserSettings(), CreateParserContext()))
                {
                    while (xmlReader.Read())
                    {
                        string path = string.Empty;
                        
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (!xmlReader.IsEmptyElement)
                                    elements.Push(xmlReader.LocalName);
                                break;
                            case XmlNodeType.EndElement:
                                if (elements.Count > 0)
                                    elements.Pop();
                                break;
                            case XmlNodeType.Attribute:
                                if (!xmlReader.IsEmptyElement)
                                    elements.Push($"@{xmlReader.LocalName}");
                                path = string.Join("/", elements.Reverse());
                                if (!tree.ContainsKey(path))
                                    tree.Add(path, xmlReader.ReadString());

                                if (elements.Count > 0)
                                    elements.Pop();
                                break;
                            case XmlNodeType.Text:
                                path = string.Join("/", elements.Reverse());
                                if (!tree.ContainsKey(path))
                                {
                                    tree.Add(path, xmlReader.ReadString());
                                    if (elements.Count > 0)
                                        elements.Pop();
                                }

                                break;
                        }
                    }
                }
            }

            Trace("BuildTree result", tree);
            return tree;
        }
        private StreamReader GetStream()
        {
            if (this._inputFile != null)
            {
                Trace(String.Format("Input File : {0} ", this._inputFile));
                return new StreamReader(this._inputFile);
            }
            else if (this._inputXmlSource != null)
            {
                Trace(String.Format("Xml Source: {0} ", this._inputXmlSource));

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(_inputXmlSource));

                return new StreamReader(stream);
            }
            else if (this._resourceFilename != null)
            {
                Trace(String.Format("Resource file name: {0} ", this._resourceFilename));
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetName().Name + "." + this._resourceFilename;

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                return new StreamReader(stream);
            }
            else
                throw new InvalidOperationException("No file/stream set.");
        }
    }
}

