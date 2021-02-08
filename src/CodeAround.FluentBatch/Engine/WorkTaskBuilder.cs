using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Destination;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Destination;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Task.Source;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Engine
{
    public class WorkTaskBuilder : ObjectBase, ITaskBuilder
    {
        private IWorkTask _workTask;
        private string _name;

        public WorkTaskBuilder(ILogger logger, bool useTrace)
           : base(logger, useTrace)
        {
        }

        public string WorkTaskName { get { return _name; } }

        public ICustomWorkTask Create<T>() where T : IWorkTask
        {
            Trace(String.Format("Create '{0}' work task type", nameof(T)));

            var workTask = (IWorkTask)Activator.CreateInstance(typeof(T), Logger, UseTrace);

            if (workTask == null)
            {
                Trace("Type error, you cannot create instance. Work Task is null");
                throw new InvalidOperationException("Type error, you cannot create instance");
            }

            _workTask = workTask;
            _workTask.Name = _name;

            return (ICustomWorkTask)workTask;
        }

        public ISqlWorkTask CreateSql()
        {
            Trace("Create Sql Work task");

            _workTask = new SqlWorkTask(Logger, UseTrace);
            _workTask.Name = _name;

            return (SqlWorkTask)_workTask;
        }


        public ITextSource CreateTextSource()
        {
            Trace("Create text source");

            _workTask = new TextSource(Logger, UseTrace);
            _workTask.Name = _name;
            return (TextSource)_workTask;
        }

        public ITaskBuilder Name(string name)
        {
            if (!String.IsNullOrEmpty(name))
                _name = name;

            return this;
        }

        public IObjectSource CreateObjectSource()
        {
            Trace("Create Object source");

            _workTask = new ObjectSource(Logger, UseTrace);
            _workTask.Name = _name;

            return (IObjectSource)_workTask;
        }

        public ISqlSource CreateSqlSource()
        {
            Trace("Create Sql source");

            _workTask = new SqlSource(Logger, UseTrace);
            _workTask.Name = _name;

            return (ISqlSource)_workTask;
        }

        public ISqlDestination CreateSqlDestination()
        {
            Trace("Create sql destination");

            _workTask = new SqlDestination(Logger, UseTrace);
            _workTask.Name = _name;

            return (SqlDestination)_workTask;
        }

        public ITextDestination CreateTextDestination()
        {
            Trace("Create text destination");

            _workTask = new TextDestination(Logger, UseTrace);
            _workTask.Name = _name;

            return (ITextDestination)_workTask;
        }

        public ILoopWorkTask<T> CreateLoop<T>()
        {
            Trace("Create loop work task type. ");

            _workTask = new LoopWorkTask<T>(Logger, UseTrace);
            _workTask.Name = _name;

            return (ILoopWorkTask<T>)_workTask;
        }

        public IConditionWorkTask<T> CreateCondition<T>()
        {
            Trace("Create condition work task type. ");

            _workTask = new ConditionWorkTask<T>(Logger, UseTrace);
            _workTask.Name = _name;

            return (IConditionWorkTask<T>)_workTask;
        }

        public IXmlSource CreateXmlSource()
        {
            Trace("Create xml source");

            _workTask = new XmlSource(Logger, UseTrace);
            _workTask.Name = _name;

            return (IXmlSource)_workTask;
        }

        public IJsonSource<T> CreateJsonSource<T>()
        {
            Trace("Create xml source");

            _workTask = new JsonSource<T>(Logger, UseTrace);
            _workTask.Name = _name;

            return (IJsonSource<T>)_workTask;
        }

        public IXmlDestination CreateXmlDestination()
        {
            Trace("Create xml destination");

            _workTask = new XmlDestination(Logger, UseTrace);
            _workTask.Name = _name;

            return (IXmlDestination)_workTask;
        }

        public IExcelSource CreateExcelSource()
        {
            Trace("Create Excel source");

            _workTask = new ExcelSource(Logger, UseTrace);
            _workTask.Name = _name;

            return (IExcelSource)_workTask;
        }

        public IWorkTask GetCurrentTask()
        {
            return _workTask;
        }
    }
}
