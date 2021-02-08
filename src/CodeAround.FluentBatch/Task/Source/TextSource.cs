using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class TextSource : WorkTask, ITextSource
    {
        private TextReader InputReader { get; set; }
        private string InputFile { get; set; }
        private bool UseHeaderFlag { get; set; }
        private int[] FixedColumnWidths { get; set; }
        private Func<string, IEnumerable<string>> LineSplitter { get; set; }
        private Func<string, bool> DoSkipWhen { get; set; }

        private List<string> _otherFields;

        public TextSource(ILogger logger, bool useTrace)
           : base(logger, useTrace)
        {
            this.DoSkipWhen = line => line.Length == 0;
            _otherFields = new List<string>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize TextSource", taskResult);
            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                InputFile = ((LoopTaskResult)TaskResult).LoopValue.ToString();
            }
        }

        public ITextSource From(TextReader reader)
        {
            if (reader == null)
            {
                Trace("text reader is null");
                throw new ArgumentNullException("text reader");
            }
            Trace("Set text reader");
            this.InputReader = reader;
            return this;
        }

        public ITextSource From(string file)
        {
            if (String.IsNullOrEmpty(file))
            {
                Trace("FIle is null");
                throw new ArgumentNullException("file");
            }

            this.InputFile = file;
            Trace("Set File", file);
            return this;
        }

        public ITextSource SkipWhen(Func<string, bool> func)
        {
            if (func == null)
            {
                Trace("func skip when");
                throw new ArgumentNullException("func skip when");
            }

            this.DoSkipWhen = func;
            Trace("Set DoSkipWhen");
            return this;
        }

        public ITextSource ParseAsDelimited(char delimiter, bool useHeader)
        {
            if (Char.IsWhiteSpace(delimiter))
            {
                Trace("delimiter is null");
                throw new ArgumentNullException("delimiter");
            }

            this.LineSplitter = (line => line.Split(delimiter).Select(p => p.Trim()).ToList());
            this.UseHeaderFlag = useHeader;
            Trace("Set ParseAsDelimited", new { Delimiter = delimiter, UseHeader = useHeader });
            return this;
        }

        public ITextSource ParseAsDelimited(char[] delimiters, bool useHeader)
        {
            if (delimiters == null || delimiters.Length == 0)
            {
                Trace("delimiter is null");
                throw new ArgumentNullException("delimiters");
            }

            this.LineSplitter = (line => line.Split(delimiters));
            this.UseHeaderFlag = useHeader;
            Trace("Set ParseAsDelimited", new { Delimiters = delimiters, UseHeader = useHeader });
            return this;
        }

        public ITextSource ParseAsFixedColumns(int[] colWidths, bool useHeader)
        {
            if (colWidths == null || colWidths.Length == 0)
            {
                Trace("colWidths is null");
                throw new ArgumentNullException("colWidths");
            }

            this.FixedColumnWidths = colWidths;
            this.LineSplitter = (line =>
            {
                IList<string> values = new List<string>();
                int ptr = 0;
                for (int idx = 0; idx < this.FixedColumnWidths.Length; idx++)
                {
                    int w = this.FixedColumnWidths[idx];
                    if (ptr + w >= line.Length)
                        values.Add(line.Substring(ptr).TrimEnd());
                    else
                        values.Add(line.Substring(ptr, w).TrimEnd());
                    ptr += w;
                    if (ptr >= line.Length)
                    {
                        break;
                    }
                }
                return values;
            });
            this.UseHeaderFlag = useHeader;
            Trace("Set ParseAsFixedColumns", new { ColWidths = colWidths, UseHeader = useHeader });
            return this;
        }

        public ITextSource ParseWith(Func<string, IEnumerable<string>> lineSplitter, bool useHeader)
        {
            if (lineSplitter == null)
            {
                Trace("line splitter is null");
                throw new ArgumentNullException("line splitter");
            }

            this.LineSplitter = lineSplitter;
            this.UseHeaderFlag = useHeader;
            Trace("Set ParseWith", new { LineSplitter = lineSplitter, UseHeader = useHeader });
            return this;
        }

        public ITextSource AddOtherField(Func<string> fieldName)
        {
            var field = fieldName();

            if (!_otherFields.Contains(field))
                _otherFields.Add(field);

            Trace("Set add other field", _otherFields);

            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;

            try
            {
                Trace("Start Text Source Execute");
                List<IRow> lst = new List<IRow>();

                if (this.InputReader != null)
                {
                    foreach (IRow r in ReadRows(this.InputReader))
                        lst.Add(r);
                }
                else if (this.InputFile != null)
                {
                    Trace("Input File {0}", this.InputFile);
                    using (StreamReader rdr = new StreamReader(this.InputFile))
                    {
                        foreach (IRow r in ReadRows(rdr))
                            lst.Add(r);
                    }
                }
                else
                    throw new InvalidOperationException("No file/stream set.");

                Trace("End Text Source Execute", lst);

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


        private IEnumerable<IRow> ReadRows(TextReader rdr)
        {
            string line;
            IList<string> headers = null;
            if (this.UseHeaderFlag)
            {
                do
                {
                    line = rdr.ReadLine();
                } while (line != null && this.DoSkipWhen(line));
                if (line == null)
                    throw new InvalidDataException("Header line is empty");
                headers = LineSplitter(line).ToList();
                Trace("Headers", headers);
            }
            while ((line = rdr.ReadLine()) != null)
            {
                if (!this.DoSkipWhen(line))
                {
                    IList<string> tokens = LineSplitter(line).ToList();
                    IDictionary<string, object> values = new Dictionary<string, object>();
                    if (headers != null)
                    {
                        for (int idx = 0; idx < tokens.Count; idx++)
                        {
                            values[headers[idx]] = tokens[idx].Trim();
                        }
                    }
                    else
                    {
                        for (int idx = 0; idx < tokens.Count; idx++)
                        {
                            values["Field" + idx] = tokens[idx].Trim();
                        }

                        for (int i = 0; i < _otherFields.Count; i++)
                        {
                            values[_otherFields[i]] = null;
                        }
                    }
                    Trace("Values", values);
                    yield return new DictionaryRow(values);
                }
            }
        }

        
    }
}