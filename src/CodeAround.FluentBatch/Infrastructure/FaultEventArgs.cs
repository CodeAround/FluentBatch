using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Task;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class FaultEventArgs
    {
        public IWorkTask WorkTask { get; set; }

        public Exception CurrentException { get; set; }

        public object TaskResult { get; set; }

        public FaultEventArgs(IWorkTask workTask, TaskResult taskResult, Exception ex)
        {
            CurrentException = ex;
            WorkTask = workTask;
            TaskResult = taskResult;
        }
    }
}
