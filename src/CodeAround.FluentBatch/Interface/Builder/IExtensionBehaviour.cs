using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeAround.FluentBatch.Interface.Builder
{
    public interface IExtensionBehaviour
    {
        IWorkTask GetCurrentTask();
        bool UseTrace { get; }
        ILogger Logger { get; }

        string WorkTaskName { get; }
    }
}
