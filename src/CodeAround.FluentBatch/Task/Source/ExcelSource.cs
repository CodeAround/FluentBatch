using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class ExcelSource : WorkTask, IExcelSource
    {
        private string _inputFile;
        private bool _usingHeader;
        private List<string> _listColumn;
        private string _sheet;
        public ExcelSource(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
            _listColumn = new List<string>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize ExcelSource", taskResult);
            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                _inputFile = ((LoopTaskResult)TaskResult).LoopValue.ToString();
            }
        }


        public IExcelSource FromFile(string fileInput)
        {
            if (String.IsNullOrEmpty(fileInput))
                throw new ArgumentNullException("File path is null or empty");
            Trace($"Set File open : {fileInput}");

            _inputFile = fileInput;
            return this;
        }

        public IExcelSource UseHeader(bool usingHeader)
        {
            Trace($"Set Use Header :{usingHeader}");
            _usingHeader = usingHeader;
            return this;
        }

        public IExcelSource Use(Func<string> columnName)
        {
            if (columnName == null)
                throw new ArgumentNullException("No column name has been set");

            string name = columnName();
            if (!_listColumn.Contains(name))
                _listColumn.Add(name);

            Trace($"Set Use", _listColumn);
            return this;
        }

        public IExcelSource Sheet(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("No Sheet name has been set");

            _sheet = name;
            Trace($"Set Sheet", _sheet);
            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;
            try
            {
                List<IEnumerable<IRow>> lst = new List<IEnumerable<IRow>>();
                using (var stream = File.Open(_inputFile, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = _usingHeader
                            }
                        });

                        Trace("Excel dataset", dataSet);

                        if (String.IsNullOrEmpty(_sheet))
                        {
                            for (int i = 0; i < dataSet.Tables.Count; i++)
                            {
                                DataTable table = dataSet.Tables[i];
                                var rows = ProcessRows(table);
                                Trace("Processed rows", rows);
                                if (!lst.Contains(rows))
                                {
                                    lst.Add(rows);
                                }
                            }
                        }
                        else
                        {
                            DataTable table = dataSet.Tables[_sheet];
                            var rows = ProcessRows(table);
                            Trace("Processed rows", rows);
                            if (!lst.Contains(rows))
                            {
                                lst.Add(rows);
                            }
                        }
                    }
                }
                result = new TaskResult(true, lst);
            }

            catch (Exception ex)
            {
                Trace($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
                throw;
            }
            return result;
        }


        public IEnumerable<IRow> ProcessRows(DataTable table)
        {
            List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
            Dictionary<string, object> dicRow = null;
            if (_usingHeader)
            {
                if (_listColumn.Count > 0)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        dicRow = new Dictionary<string, object>();

                        foreach (var column in _listColumn)
                        {
                            var row = table.Rows[i][column];

                            if (!dicRow.ContainsKey(column))
                            {
                                dicRow.Add(column, row);
                            }
                        }

                        values.Add(dicRow);
                        Trace("Values", values);
                    }
                }
                else
                {
                    throw new ArgumentNullException("No column name has been set");
                }

            }
            else
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dicRow = new Dictionary<string, object>();
                    var row = table.Rows[i].ItemArray.ToList();
                    for (int j = 0; j < row.Count; j++)
                    {
                        var item = row[j];
                        if (!dicRow.ContainsKey(j.ToString()))
                        {
                            dicRow.Add(j.ToString(), item);
                        }

                    }
                    values.Add(dicRow);
                    Trace("Values", values);
                }

            }

            return values.Select(x => new DictionaryRow(x)).ToList();
        }
    }
}
