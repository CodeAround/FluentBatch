
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Generic
{
    public abstract class WorkTask : ObjectBase, IWorkTask, IFault
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }


        protected Action<FaultEventArgs> _faultTask;

        public TaskResult TaskResult { get; private set; }

        public Dictionary<string, object> PreviousTaskResult { get; set; }

        public Dictionary<string, object> ParentPreviousTaskResult { get; set; }

        public TaskResult ParentTaskResult { get; private set; }

        public WorkTask(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            Id = Guid.NewGuid();
            PreviousTaskResult = new Dictionary<string, object>();
        }

        public virtual void Initialize(TaskResult taskResult)
        {
            TaskResult = taskResult;
        }

        public void InitParentResult(IWorkTask workTask)
        {
            if(workTask != null && workTask.PreviousTaskResult != null)
            {
                ParentPreviousTaskResult = workTask.PreviousTaskResult;
                ParentTaskResult = workTask.TaskResult;
            }
        }

        public void InitPreviousResult(string taskName, Dictionary<string, object> previousResult)
        {
            if (previousResult != null)
            {
                foreach (var keyValuePair in previousResult)
                {
                    if (!PreviousTaskResult.ContainsKey(keyValuePair.Key))
                    {
                        PreviousTaskResult.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                if (!PreviousTaskResult.ContainsKey(taskName))
                    PreviousTaskResult.Add(taskName, TaskResult);
            }
        }

        public virtual TaskResult Execute()
        {
            return null;
        }

        public virtual void Finish()
        {

        }

        public IFault Fault(Action<FaultEventArgs> faultTask)
        {
            if (faultTask == null)
            {
                Trace("fault task is null");
                throw new ArgumentNullException("fault task event");
            }

            _faultTask = faultTask;
            return this;
        }

        protected void Fault(Exception ex)
        {
            if (_faultTask != null)
                _faultTask(new FaultEventArgs(this, TaskResult, ex));
        }

        public IWorkTask Build()
        {
            return this as IWorkTask;
        }
    }
}
