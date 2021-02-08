using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Base;

namespace CodeAround.FluentBatch.Test.TestUtilities
{
    public class TextCustomWorkTask : CustomWorkTaskBase
    {
        public Guid Id { get; }
        public object TaskResult { get; }
        private IEnumerable<IRow> _textSource;
        private IDictionary<string, object> values;
        public TextCustomWorkTask(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
        }
        public override void Initialize(TaskResult taskResult)
        {
            _textSource = (IEnumerable<IRow>)taskResult.Result;
        }


        public override TaskResult Execute()
        {
            List<IRow> result = new List<IRow>();
            values = new Dictionary<string, object>();
            var fieldsValue = new List<object>();
            foreach (var row in _textSource)
            {
                for (int i = 0; i < row.Fields.Count(); i++)
                {
                    var key = row.Fields.ToList()[i];
                    var value = row[key];
                    values[key] = value;
                }
                var dic = new DictionaryRow(values);
                dic.Operation = OperationType.Insert;
                result.Add(dic);
            }
            
            
            return new TaskResult(true, result);
        }

        public ICustomWorkTask AddParameter(string name, object value)
        {
            return this;
        }

        public void Finish()
        {

        }
    }
}
