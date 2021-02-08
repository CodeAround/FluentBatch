using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Engine
{
    public class FlowBuilder : ObjectBase
    {
        public FlowBuilder(ILogger logger, bool useTrace = false)
            : base(logger, useTrace)
        {
        }

        private FlowContext _context;
        public FlowBuilder Create(string name)
        {
            _context = new FlowContext(Logger, UseTrace);
            _context.Name = name;
            Trace("Flow Builder - Create: FlowContex", _context.Name);
            return this;
        }
        public FlowBuilder Then(Func<ITaskBuilder, IWorkTask> taskFunc, int position = -1)
        {
            Trace($"Flow Builder - Then : workTask is null: { taskFunc is null}");
            if (taskFunc != null)
            {
                Trace("Flow Builder - Then");
                WorkTaskBuilder builder = new WorkTaskBuilder(Logger, UseTrace);
                var workTask = taskFunc.Invoke(builder);
                Trace(String.Format($"Flow Builder - Then : workTask is null: {workTask is null}"));
                if (workTask != null)
                    _context.AddTask(workTask, position);
            }

            return this;
        }

        public FlowContext Build()
        {
            Trace(String.Format("Flow Builder - Build : Context: {0}", _context.Name));
            return _context;
        }
    }
}
