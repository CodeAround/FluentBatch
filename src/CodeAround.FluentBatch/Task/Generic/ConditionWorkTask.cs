using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CodeAround.FluentBatch.Task.Generic
{
    public class ConditionWorkTask<T> : WorkTask, IConditionWorkTask<T>
    {
        private List<IWorkTask> _thenTask;
        private List<IWorkTask> _elseTask;
        private Func<T, bool> _condition;
        private bool _usePreviousTaskResult;
        private T _objToValidate;

        public event EventHandler<WorkTaskEventArgs> ProcessingTask;
        public event EventHandler<WorkTaskEventArgs> ProcessedTask;

        public ConditionWorkTask(ILogger logger, bool useTrace)
        : base(logger, useTrace)
        {
            _usePreviousTaskResult = true;
            _thenTask = new List<IWorkTask>();
            _elseTask = new List<IWorkTask>();
        }

        public IConditionWorkTask<T> UsePreviousTaskResult()
        {
            Trace("Set UsePreviousTaskResult");
            _usePreviousTaskResult = true;

            return this;
        }

        public IConditionWorkTask<T> ValidationSource(T obj)
        {
            Trace("Set ValidationSource", obj);
            _objToValidate = obj;
            _usePreviousTaskResult = false;
            return this;
        }

        public IConditionWorkTask<T> If(Func<T, bool> condition)
        {
            Trace("Set If");
            _condition = condition;

            return this;
        }

        public IConditionWorkTask<T> Then(Func<ITaskBuilder, IWorkTask> taskFunc)
        {
            var builder = new WorkTaskBuilder(Logger, UseTrace);
            var task = taskFunc(builder);

            Trace(String.Format($"Set Them, Task is null : {task is null}"));
            if (task != null)
            {
                _thenTask.Add(task);
            }

            return this;
        }

        public IConditionWorkTask<T> Else(Func<ITaskBuilder, IWorkTask> taskFunc)
        {
            var builder = new WorkTaskBuilder(Logger, UseTrace);
            var task = taskFunc(builder);

            Trace(String.Format($"Set Else Task is null : {task is null}"));
            if (task != null)
            {
                _elseTask.Add(task);
            }

            return this;
        }

        public IConditionWorkTask<T> ProcessingTaskEvent(Action<object, WorkTaskEventArgs> processingTask)
        {
            if (processingTask == null)
            {
                Trace("processingTask task event is null or empty");
                throw new ArgumentNullException("processingTask task event");
            }

            ProcessingTask += (s, e) => processingTask(s, e);

            return this;
        }

        public IConditionWorkTask<T> ProcessedTaskEvent(Action<object, WorkTaskEventArgs> processedTask)
        {
            if (processedTask == null)
            {
                Trace("input parameter is null or empty");
                throw new ArgumentNullException("processingTask task event");
            }

            ProcessedTask += (s, e) => processedTask(s, e);

            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;
            try
            {
                Trace(String.Format($"Condition is null : {_condition is null}"));
                TaskResult taskResult = null;
                Dictionary<string, object> previousTaskResult = null;
                string previousTaskName = null;
                T currentObj = _objToValidate;

                if (_usePreviousTaskResult)
                {
                    Trace("Execute usePreviousTaskResult", TaskResult);
                    if (TaskResult is TaskResult && TaskResult.Result is T)
                    {
                        currentObj = (T)TaskResult.Result;
                    }
                    else if (TaskResult is LoopTaskResult && ((LoopTaskResult)TaskResult).LoopValue is T)
                    {
                        currentObj = (T)((LoopTaskResult)TaskResult).LoopValue;
                    }
                    else
                    {
                        Trace(String.Format($"Previous Task Result il not {typeof(T).FullName}"));
                        throw new ArgumentException($"Task Result should be of type  '{typeof(T).FullName}'");
                    }
                }

                bool condition = _condition(currentObj);

                if (condition)
                {
                    Trace("Condition = True");
                    if (_thenTask != null && _thenTask.Count > 0)
                    {
                        foreach (var workTask in _thenTask)
                        {
                            Trace("Condition = True. WorkTask", workTask);
                            OnProcessingTask(workTask, taskResult);

                            workTask.InitPreviousResult(previousTaskName, previousTaskResult);
                            workTask.Initialize(taskResult);
                            previousTaskName = workTask.GetType().FullName;
                            previousTaskResult = workTask.PreviousTaskResult;
                            taskResult = workTask.Execute();
                            workTask.Finish();

                            if (taskResult == null)
                                throw new ArgumentNullException("Task Result is null");

                            if (!taskResult.IsCompleted)
                            {
                                Log($"Task Result is not completd. Task Esult: {JsonConvert.SerializeObject(taskResult, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
                                break;
                            }
                            else
                            {
                                OnProcessedTask(workTask, taskResult);
                            }
                        }
                    }
                }
                else
                {
                    Trace("Condition = False");
                    if (_elseTask != null && _elseTask.Count > 0)
                    {
                        foreach (var workTask in _elseTask)
                        {
                            Trace("Condition = False. WorkTask", workTask);
                            OnProcessingTask(workTask, taskResult);

                            workTask.InitPreviousResult(previousTaskName, previousTaskResult);
                            workTask.Initialize(taskResult);
                            previousTaskName = workTask.GetType().FullName;
                            previousTaskResult = workTask.PreviousTaskResult;
                            taskResult = workTask.Execute();
                            workTask.Finish();

                            if (taskResult == null)
                                throw new ArgumentNullException("Task Result is null");

                            if (!taskResult.IsCompleted)
                            {
                                Log($"Task Result is not completd. Task Result: {JsonConvert.SerializeObject(taskResult, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
                                break;
                            }
                            else
                            {
                                OnProcessedTask(workTask, taskResult);
                            }
                        }
                    }
                }

                result = new TaskResult(true, null);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
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
    }
}
