using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Base;
using CodeAround.FluentBatch.Task.Generic;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.Infrastructure
{
    class CustomWorkTask : CustomWorkTaskBase
    {
        public Guid Id { get; }
        public object TaskResult { get; }
        private LoopTaskResult _persons;
        private IEnumerable<IRow> _cardEnumerable;
        public CustomWorkTask(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
        }

        public override void Initialize(TaskResult taskResult)
        {
            if (taskResult is LoopTaskResult)
            {
                _persons = (LoopTaskResult)taskResult;
            }
            else
            {
                _cardEnumerable = (IEnumerable<IRow>)taskResult.Result;
            }
        }

        public override TaskResult Execute()
        {
            if (_persons != null)
            {
                var card = (Persons)_persons.LoopValue;
                var rows = GetRows(card);
                return new TaskResult(true, rows.ToList());
            }
            else
            {
                var rows = GetRows(_cardEnumerable);
                return new TaskResult(true, rows.ToList());
            }
        }

        private IEnumerable<IRow> GetRows(object obj)
        {
            if (obj is Persons)
            {
                Persons newPerson = (Persons)obj;
                DataTable dt = new DataTable();
                dt.Columns.Add("PersonId", typeof(int));
                dt.Columns.Add("Name");
                dt.Columns.Add("Surname");
                dt.Columns.Add("BirthdayDate", typeof(DateTime));
                dt.Rows.Add(newPerson.PersonId, newPerson.Name, newPerson.Surname, newPerson.BirthdayDate);
                foreach (DataRow row in dt.Rows)
                {
                    DataReaderRow rowInsert = new DataReaderRow(row);
                    rowInsert.Operation = OperationType.Insert;
                    yield return rowInsert;
                }
            }
            else
            {
                var rowInsertRows = (IEnumerable<IRow>)obj;
                foreach (var row in rowInsertRows)
                {
                    row.Operation = OperationType.Insert;
                    yield return row;
                }
            }
        }

        public ICustomWorkTask AddParameter(string name, object value)
        {
            return this;
        }
    }
}
