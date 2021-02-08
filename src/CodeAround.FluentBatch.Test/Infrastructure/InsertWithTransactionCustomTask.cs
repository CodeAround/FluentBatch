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
    public class InsertWithTransactionCustomTask : CustomWorkTaskBase
    {
        private IEnumerable<IRow> _rows;
        public InsertWithTransactionCustomTask(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {

        }

        public override void Initialize(TaskResult taskResult)
        {
            base.Initialize(taskResult);
            _rows = (IEnumerable<IRow>)taskResult.Result;
        }

        public override TaskResult Execute()
        {
            if (_rows != null && _rows.Count() > 0)
            {
                foreach (var row in _rows)
                {
                    row.Operation = OperationType.Insert;

                    if (row["Surname"].ToString() == "TEST")
                    {
                        row["BirthdayDate"] = DateTime.MinValue;
                    }
                }
            }
            return new TaskResult(true, _rows);
        }
    }
}
