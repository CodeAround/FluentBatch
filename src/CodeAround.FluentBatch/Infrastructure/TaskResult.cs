using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class TaskResult
    {
        public object Result { get; set; }

        public bool IsCompleted { get; set; }

        public TaskResult(bool isCompleted, object result)
        {
            Result = result;
            IsCompleted = isCompleted;
        }
    }
}
