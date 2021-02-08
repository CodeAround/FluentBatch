using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Destination
{
    public class TextDestination : WorkTask, ITextDestination
    {
        public class RowItem
        {
            private string _name;
            private object _value;

            public string Name
            {
                get { return _name; }
            }

            public object Value
            {
                get { return _value; }
            }

            public RowItem(string name, object value)
            {
                _name = name;
                _value = value;
            }
        }

        public TextDestination(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
        }


        private TextWriter OutputWriter { get; set; }
        private string OutputFile { get; set; }
        private bool WriteHeaderFlag { get; set; }
        private IEnumerable<string> FieldNames { get; set; }
        private IEnumerable<IRow> _aRows { get; set; }
        private Func<IRow, string> WriteLine { get; set; }


        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize TextDestination", taskResult);

            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                _aRows = (IEnumerable<IRow>)((LoopTaskResult)TaskResult).Result;
            }
            else
            {
                _aRows = (IEnumerable<IRow>)TaskResult.Result;
            }
        }

        public ITextDestination To(TextWriter writer)
        {
            Trace("Set To text writer");
            if (writer == null)
                throw new ArgumentNullException("File is null");

            this.OutputWriter = writer;
            return this;
        }

        public ITextDestination To(string file)
        {
            Trace("Set To file", file);
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException("File is null");

            this.OutputFile = file;
            return this;
        }

        public ITextDestination WithHeader(bool b)
        {
            Trace("Set WithHeader", b);
            this.WriteHeaderFlag = b;
            return this;
        }

        public ITextDestination SetFieldNames(IEnumerable<string> fieldNames)
        {
            Trace("Set Field names", fieldNames);
            this.FieldNames = fieldNames;
            return this;
        }

        public ITextDestination WriteAsFixedColumns(int[] columnWidths, string separator)
        {
            Trace("Set Write as Fixed Columns", new { ColumnWidths = columnWidths, Separator = separator });

            return this.WriteAs(rowItems =>
            {
                IList<string> parts = new List<string>();
                int ctr = 0;
                foreach (var item in rowItems)
                {
                    parts.Add((item.Value ?? "").ToString().PadRight(columnWidths[ctr++]));
                }
                return string.Join(separator, parts.ToArray());
            });
        }

        public ITextDestination WriteAsFixedColumns(int columnWidth, string separator)
        {
            Trace("Set Write as Fixed Columns int columnn width", new { ColumnWidth = columnWidth, Separator = separator });
            return this.WriteAs(rowItems =>
            {
                IList<string> parts = new List<string>();
                foreach (var item in rowItems)
                {
                    parts.Add((item.Value ?? "").ToString().PadRight(columnWidth));
                }
                return string.Join(separator, parts.ToArray());
            });
        }

        public ITextDestination WriteAsDelimited(string separator)
        {
            Trace("Set Write as delimiter", separator);

            this.WriteLine = (row) =>
            {
                IList<string> parts = new List<string>();
                foreach (var item in EnumOrderedRowItems(row))
                {
                    parts.Add((item.Value ?? "").ToString());
                }
                return string.Join(separator, parts.ToArray());
            };
            return this;
        }

        public ITextDestination WriteAs(Func<IEnumerable<RowItem>, string> funcFormatter)
        {
            Trace("Set Write as func formatter");

            this.WriteLine = (row) =>
            {
                return funcFormatter(EnumOrderedRowItems(row));
            };
            return this;
        }

        public override TaskResult Execute()
        {
            TextWriter textWriter = null;
            TaskResult result = null;
            try
            {
                if (_aRows != null)
                {
                    Trace("Processing Rows", _aRows);

                    if (this.OutputFile != null)
                    {
                        textWriter = new StreamWriter(this.OutputFile);
                    }
                    else if (this.OutputWriter != null)
                    {
                        textWriter = this.OutputWriter;
                    }
                    else
                        throw new InvalidOperationException("No file/stream set.");

                    if (textWriter != null)
                    {
                        using (textWriter)
                        {
                            if (this.WriteHeaderFlag)
                            {
                                DictionaryRow headerRow = new DictionaryRow();
                                foreach (string field in this.FieldNames)
                                    headerRow[field] = field;

                                Trace("Processing Header", headerRow);

                                textWriter.WriteLine(this.WriteLine(headerRow));
                            }

                            foreach (var aRow in _aRows)
                            {
                                Trace("Processing Row", aRow);
                                textWriter.WriteLine(this.WriteLine(aRow));
                            }
                        }
                    }
                }
                result = new TaskResult(true, _aRows);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }

        private IEnumerable<RowItem> EnumOrderedRowItems(IRow row)
        {
            foreach (string field in this.FieldNames)
            {
                yield return new RowItem(field, row[field]);
            }
        }

    }
}