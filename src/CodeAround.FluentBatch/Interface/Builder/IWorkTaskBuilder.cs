using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Task.Source;

namespace CodeAround.FluentBatch.Interface.Builder
{
    public interface IWorkTaskBuilder
    {
        ICustomWorkTask Create<T>() where T : IWorkTask;

        ILoopWorkTask<T> CreateLoop<T>();

        IConditionWorkTask<T> CreateCondition<T>();

        ISqlWorkTask CreateSql();

    }
}
