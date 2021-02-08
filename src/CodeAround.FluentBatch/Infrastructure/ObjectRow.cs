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
    public class ObjectRow : IRow
    {
        private object _obj;
        private IDictionary<string, PropertyInfo> _props;

        public ObjectRow(object o, IDictionary<string, PropertyInfo> props)
        {
            if (o == null)
                throw new InvalidOperationException("Cannot create object row from null.");
            _obj = o;
            _props = props;
        }

        public OperationType Operation { get; set; }

        public object this[string field]
        {
            get
            {
                return _props[field].GetValue(_obj, null);
            }
            set
            {
                _props[field].SetValue(_obj, value);
            }
        }

        public IEnumerable<string> Fields
        {
            get
            {
                return _props.Keys;
            }
        }

        public bool ContainsField(string field)
        {
            return _props.ContainsKey(field);
        }
    }
}
