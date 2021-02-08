using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class LoopTaskResult : TaskResult
    {
        public object LoopValue { get; set; }

        public LoopTaskResult(TaskResult result, object loopValue)
        : base(result.IsCompleted, result.Result)
        {
            LoopValue = loopValue;
        }
    }
}
