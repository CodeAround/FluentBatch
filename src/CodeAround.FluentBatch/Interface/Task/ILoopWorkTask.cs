
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Task
{
    public interface ILoopWorkTask<T> : IFault
    {
        ILoopWorkTask<T> Append(Func<ITaskBuilder, IWorkTask> taskFunc, int position = -1);

        ILoopWorkTask<T> AddLoop(IEnumerable<T> list);

        ILoopWorkTask<T> UseParallelProcess(int maxDegree);

        ILoopWorkTask<T> ProcessedTaskEvent(Action<object, WorkTaskEventArgs> processedTask);

        ILoopWorkTask<T> ProcessingTaskEvent(Action<object, WorkTaskEventArgs> processingTask);

        ILoopWorkTask<T> IsEmpty(Func<bool> isEmpty);

        ILoopWorkTask<T> Loop<TCurrent>(Func<List<T>> getNext);
    }
}
