using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Base;
using CodeAround.FluentBatch.Infrastructure;

namespace CodeAround.FluentBatch.Test.Infrastructure
{
    public class UpdateCustomWorkTask : CustomWorkTaskBase
    {
        public Guid Id { get; }
        public object TaskResult { get; }

        private IEnumerable<IRow> _rowEnum;

        public UpdateCustomWorkTask(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {

        }

        public override void Initialize(TaskResult taskResult)
        {
            base.Initialize(taskResult);
            _rowEnum = (IEnumerable<IRow>)taskResult.Result;
        }

        public override TaskResult Execute()
        {
            var taskResult = new Dictionary<string, object>();
            if (OtherFields.Any())
            {
                List<IRow> rows = new List<IRow>();


                foreach (var t in _rowEnum)
                {
                    rows.Add(UpdateValue(t));
                }
                return new TaskResult(true, rows.ToList());
            }
            else
            {
                var rows = DeleteValue(_rowEnum);
                return new TaskResult(true, rows.ToList());
            }

        }

        private IRow UpdateValue(IRow row)
        {
            foreach (var r in OtherFields)
            {
                if (row.ContainsField(r.Key))
                {
                    row.Operation = OperationType.Update;
                    row[r.Key] = r.Value;

                }
            }
            return row;
        }

        private IEnumerable<IRow> DeleteValue(object card)
        {
            var rowInsertRows = (IEnumerable<IRow>)card;
            foreach (var row in rowInsertRows)
            {
                row.Operation = OperationType.Delete;
                yield return row;
            }

        }

        public void Finish()
        {

        }
    }
}
