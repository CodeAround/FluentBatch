using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Task.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Test.Infrastructure
{
    public class InsertCustomExcelTask : CustomWorkTaskBase
    {
        private IEnumerable<IRow> _rows;
        public InsertCustomExcelTask(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {

        }

        public override void Initialize(TaskResult taskResult)
        {
            base.Initialize(taskResult);
            _rows = ((List<IEnumerable<IRow>>)taskResult.Result).FirstOrDefault();
        }

        public override TaskResult Execute()
        {
            if (_rows != null && _rows.Count() > 0)
            {
                foreach (var row in _rows)
                {
                    row.Operation = OperationType.Insert;
                }
            }
            return new TaskResult(true, _rows);
        }
    }
}
