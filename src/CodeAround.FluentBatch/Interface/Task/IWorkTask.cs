using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;

namespace CodeAround.FluentBatch.Interface.Task
{
    public interface IWorkTask
    {
        Guid Id { get; }
        string Name { get; set; }
        TaskResult TaskResult { get; }

        TaskResult ParentTaskResult { get; }
        Dictionary<string, object> PreviousTaskResult { get; }

        Dictionary<string, object> ParentPreviousTaskResult { get; set; }
        void Initialize(TaskResult taskResult);
        void InitPreviousResult(string taskName, Dictionary<string, object> previousResult);

        void InitParentResult(IWorkTask workTask);
        TaskResult Execute();
        void Finish();

    }
}
