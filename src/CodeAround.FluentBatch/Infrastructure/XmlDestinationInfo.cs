using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class XmlDestinationInfo
    {
        public List<XmlDestinationInfo> Children { get; set; }
        public bool IsElement { get; set; }
        public bool IsHeader { get; set; }
        public XmlDestinationInfo Parent { get; set; }
        public string XPath { get; set; }
        public List<XmlDestinationInfo> Rows { get; set; }
        public string NodeName { get; set; }
        public XMLNodeType XmlNodeType { get; set; }
        public IFormatProvider FormatProvider { get; set; }
    }
}
