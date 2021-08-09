using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Base
{
    public class CustomWorkTaskBase : WorkTask, ICustomWorkTask
    {
        protected Dictionary<string, object> OtherFields { get; set; }

        public CustomWorkTaskBase(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            OtherFields = new Dictionary<string, object>();
        }

        public ICustomWorkTask AddParameter(string name, object value)
        {
            Trace($"add parameter", new { Name = name, Value = value });
            if (!OtherFields.ContainsKey(name))
                OtherFields.Add(name, value);

            return this;
        }

        protected T ExecuteWithCallback<T>(Action<Action<T>> callMain)
        {
            if (callMain == null)
                throw new ArgumentNullException("callMain");

            var taskCompletionSource = new TaskCompletionSource<T>();
            var task = taskCompletionSource.Task;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    callMain(taskCompletionSource.SetResult);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            }, TaskCreationOptions.AttachedToParent);

            var result = System.Threading.Tasks.Task.FromResult(task.Result).Result;
            return result;
        }

        protected T ExecuteAsync<T>(Func<T> callMain)
        {
            if (callMain == null)
                throw new ArgumentNullException("callMain");

            var result = System.Threading.Tasks.Task.Run(() => callMain()).Result;

            return result;
        }
    }
}
