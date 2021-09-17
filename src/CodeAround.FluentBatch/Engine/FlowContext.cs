using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CodeAround.FluentBatch.Engine
{
    public class FlowContext : ObjectBase, IFlowContext
    {
        public event EventHandler<WorkTaskEventArgs> ProcessingTask;
        public event EventHandler<WorkTaskEventArgs> ProcessedTask;
        public event EventHandler<Exception> FaultTask;

        private List<IWorkTask> _task;

        public string Name { get; set; }

        public FlowContext(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            _task = new List<IWorkTask>();
        }

        public void AddTask(IWorkTask task, int position = -1)
        {
            Trace($"Flow Context - AddTask : task is null: {task is null}");
            if (task != null)
            {
                Trace("Start Add Task Flow Context");

                if (position < 0)
                    _task.Add(task);
                else if (position > _task.Count)
                {
                    Trace(String.Format("Invalid index exception. The {0} index must be less or equal to number of elements (collection count {1})", position, _task.Count));
                    throw new InvalidOperationException(String.Format("Invalid index exception. The {0} index must be less or equal to number of elements (collection count {1})", position, _task.Count));
                }
                else
                    _task.Insert(position, task);

                Trace("End Add Task Flow Context");
            }
        }

        public int TaskCount => _task.Count;

        public void Run()
        {
            try
            {
                Trace("Start Flow Context Run");
                TaskResult taskResult = null;
                Dictionary<string, object> previousTaskResult = null;
                string previousTaskName = null;
                Trace(String.Format($"Flow Context - Run : task is null: {_task is null}"));
                if (_task != null)
                {
                    foreach (var workTask in _task)
                    {
                        OnProcessingTask(workTask, taskResult);

                        Trace(String.Format("Execute Task. {0} Id: {1}", workTask.GetType().Name, workTask.Id), workTask);
                        workTask.Initialize(taskResult);
                        workTask.InitPreviousResult(previousTaskName, previousTaskResult);
                        taskResult = workTask.Execute();
                        previousTaskName = workTask.GetType().FullName;
                        previousTaskResult = workTask.PreviousTaskResult;
                        Trace("Execute Task. TaskResult", workTask.TaskResult);
                        workTask.Finish();

                        if (taskResult == null)
                            throw new ArgumentNullException("Task Result is null");

                        if (!taskResult.IsCompleted)
                        {
                            Log($"Task Result is not completd.", obj: workTask.TaskResult);
                            break;
                        }
                        else
                        {
                            OnProcessedTask(workTask, taskResult);
                        }
                    }
                }

                Trace("End Flow Context Run");
            }
            catch (Exception ex)
            {
                OnFaultTask(ex);
                Log($"Flow Error: {ex.ToExceptionString()}", ex);
            }
        }


        private void OnProcessingTask(IWorkTask workTask, TaskResult currentTaskResult)
        {
            var handler = ProcessingTask;
            if (handler != null)
            {
                handler(this, new WorkTaskEventArgs(workTask, currentTaskResult));
            }
        }

        private void OnProcessedTask(IWorkTask workTask, TaskResult currentTaskResult)
        {
            var handler = ProcessedTask;
            if (handler != null)
            {
                handler(this, new WorkTaskEventArgs(workTask, currentTaskResult));
            }
        }

        private void OnFaultTask(Exception ex)
        {
            var handler = FaultTask;
            if (handler != null)
            {
                handler(this, ex);
            }
        }
    }
}

