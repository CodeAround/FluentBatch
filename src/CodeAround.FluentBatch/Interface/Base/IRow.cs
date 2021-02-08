using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CodeAround.FluentBatch.Infrastructure;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IRow
    {
        object this[string field] { get; set; }

        IEnumerable<string> Fields { get; }

        bool ContainsField(string field);

        OperationType Operation { get; set; }
    }
}
