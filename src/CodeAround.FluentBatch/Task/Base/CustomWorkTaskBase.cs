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
    }
}
