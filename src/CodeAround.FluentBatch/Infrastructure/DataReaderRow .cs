using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Base;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class DataReaderRow : IRow
    {
        private DataRow Data { get; set; }

        public DataReaderRow(DataRow dr)
        {
            this.Data = dr;
        }

        public object this[string key]
        {
            get
            {
                return this.Data[key];
            }
            set
            {
                this.Data[key] = value;
            }
        }

        public IEnumerable<string> Fields
        {
            get
            {
                for (int i = 0; i < this.Data.Table.Columns.Count; i++)
                    yield return this.Data.Table.Columns[i].ColumnName;
            }
        }

        public OperationType Operation { get; set; }

        public bool ContainsField(string field)
        {
            for (int i = 0; i < this.Data.Table.Columns.Count; i++)
                if (this.Data.Table.Columns[i].ColumnName == field)
                    return true;

            return false;
        }
    }
}
