using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Task;

namespace CodeAround.FluentBatch.Event
{
    public class WorkTaskEventArgs
    {
        public IWorkTask WorkTask { get; private set; }

        public TaskResult CurrentTaskResult { get; set; }

        public WorkTaskEventArgs(IWorkTask workTask, TaskResult currentTaskResult)
        {
            WorkTask = workTask;
            CurrentTaskResult = currentTaskResult;
        }
    }
}
