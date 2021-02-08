using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Base;
using System.Data;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class DictionaryRow : IRow
    {
        protected IDictionary<string, object> Values { get; set; }

        public OperationType Operation { get; set; }

        public DictionaryRow()
        {
            this.Values = new Dictionary<string, object>();
        }

        public DictionaryRow(IDictionary<string, object> values)
        {
            this.Values = values;
        }

        public object this[string field]
        {
            get
            {
                object val;
                this.Values.TryGetValue(field, out val);
                return val;
            }
            set
            {
                this.Values[field] = value;
            }
        }

        public IEnumerable<string> Fields
        {
            get
            {
                return this.Values.Keys;
            }
        }

        public bool ContainsField(string field)
        {
            return this.Values.ContainsKey(field);
        }
    }
}
