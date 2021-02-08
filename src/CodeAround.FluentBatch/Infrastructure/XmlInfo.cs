using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class XmlInfo
    {
        public string NodeName { get; set; }
        public XMLFormatType XmlFormatType { get; set; }
        public XMLNodeType XmlNodeType { get; set; }

        public IFormatProvider FormatProvider { get; set; }

        public string XPath { get; set; }
    }
}
