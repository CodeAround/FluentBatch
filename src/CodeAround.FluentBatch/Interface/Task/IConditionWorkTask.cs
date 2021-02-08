using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Task
{
    public interface IConditionWorkTask<T> : IFault
    {
        IConditionWorkTask<T> If(Func<T, bool> condition);
        IConditionWorkTask<T> Then(Func<ITaskBuilder, IWorkTask> taskFunc);
        IConditionWorkTask<T> Else(Func<ITaskBuilder, IWorkTask> taskFunc);
        IConditionWorkTask<T> ProcessingTaskEvent(Action<object, WorkTaskEventArgs> processingTask);
        IConditionWorkTask<T> ProcessedTaskEvent(Action<object, WorkTaskEventArgs> processedTask);
        IConditionWorkTask<T> UsePreviousTaskResult();
        IConditionWorkTask<T> ValidationSource(T obj);
    }
}
