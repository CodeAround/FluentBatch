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
        Dictionary<string, object> PreviousTaskResult { get; }
        void Initialize(TaskResult taskResult);
        void InitPreviousResult(string taskName, Dictionary<string, object> previousResult);
        TaskResult Execute();
        void Finish();

    }
}
