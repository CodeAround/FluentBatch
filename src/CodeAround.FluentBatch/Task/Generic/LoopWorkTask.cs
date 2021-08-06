using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Generic
{
    public class LoopWorkTask<T> : WorkTask, ILoopWorkTask<T>
    {
        private bool _userParallel;
        private int _maxDegree;
        private List<IWorkTask> _task;
        private IEnumerable<T> _parameters;
        public event EventHandler<WorkTaskEventArgs> ProcessingTask;
        public event EventHandler<WorkTaskEventArgs> ProcessedTask;
        private Func<List<T>> _getNext;
        Func<bool> _isEmpty;

        public LoopWorkTask(ILogger logger, bool useTrace)
            : base(logger, useTrace)

        {
            _task = new List<IWorkTask>();
        }

        public ILoopWorkTask<T> Append(Func<ITaskBuilder, IWorkTask> taskFunc, int position = -1)
        {
            var builder = new WorkTaskBuilder(Logger, UseTrace);
            var task = taskFunc(builder);

            Trace(String.Format($"Task is null : {task is null}"));
            if (task != null)
            {
                if (position < 0)
                    _task.Add(task);
                else if (position > _task.Count)
                {
                    Trace(String.Format("Invalid index exception. The {0} index must be less or equal to number of elements (collection count {1})", position, _task.Count));
                    throw new InvalidOperationException(String.Format("Invalid index exception. The {0} index must be less or equal to number of elements (collection count {1})", position, _task.Count));
                }
                else
                    _task.Insert(position, task);
            }

            return this;
        }
        public ILoopWorkTask<T> ProcessingTaskEvent(Action<object, WorkTaskEventArgs> processingTask)
        {
            if (processingTask == null)
            {
                Trace("processing Task task event is null or empty");
                throw new ArgumentNullException("processingTask task event");
            }

            ProcessingTask += (s, e) => processingTask(s, e);

            return this;
        }

        public ILoopWorkTask<T> ProcessedTaskEvent(Action<object, WorkTaskEventArgs> processedTask)
        {
            if (processedTask == null)
            {
                Trace("processed task is null or empty");
                throw new ArgumentNullException("processingTask task event");
            }

            ProcessedTask += (s, e) => processedTask(s, e);

            return this;
        }

        public ILoopWorkTask<T> AddLoop(IEnumerable<T> list)
        {
            if (list == null)
            {
                Trace("input parameter is null or empty");
                throw new ArgumentNullException("input parameter is null or empty");
            }

            _parameters = list;

            return this;
        }

        public ILoopWorkTask<T> IsEmpty(Func<bool> isEmpty)
        {
            if(isEmpty == null)
            {
                Trace("isEmpty funct is null or empty");
                throw new ArgumentNullException("isEmpty is null or empty");
            }

            _isEmpty = isEmpty;

            return this;
        }

        public ILoopWorkTask<T> Loop(Func<List<T>> getNext)
        {
            if (getNext == null)
            {
                Trace("getNext funct is null or empty");
                throw new ArgumentNullException("getNext is null or empty");
            }

            _getNext = getNext;

            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;
            try
            {
                var count = _parameters == null ? 0 : _parameters.Count();
                Trace(String.Format("Start Execute Loop task. Task count {0} and Parameter {1} ", _task.Count, count));
              
                Trace("Tasks", _task);
                Trace("Parameters", _parameters);

                if (_task.Count > 0)
                {
                    if (_parameters != null && _parameters.Count() > 0)
                    {
                        if (!_userParallel)
                        {
                            foreach (var item in _parameters)
                            {
                                ExecuteBody(item);
                            }
                        }
                        else
                        {
                            Parallel.ForEach(_parameters, new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = _maxDegree
                            }, (item) =>
                            {
                                ExecuteBody(item);
                            });
                        }
                    }
                    else if(_isEmpty != null && !_isEmpty() && _getNext != null)
                    {
                        if (!_userParallel)
                        {
                            T prevItem = default(T);
                            foreach (var item in _getNext())
                            {
                                prevItem = item;
                                ExecuteBody(item);
                            }
                        }
                        else
                        {
                            Parallel.ForEach(_getNext(), new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = _maxDegree
                            }, (item) =>
                            {
                                ExecuteBody(item);
                            });
                        }
                    }
                }

                result = new TaskResult(true, null);
                Trace("End Execute Loop task");
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }

        private void ExecuteBody<T>(T item)
        {
            TaskResult taskResult = null;
            Dictionary<string, object> previousTaskResult = null;
            string previousTaskName = null;

            foreach (var workTask in _task)
            {
                Trace("Current WorkTask", workTask);
                Trace("Parameter", item);

                if (taskResult == null || (taskResult != null && taskResult.IsCompleted))
                {
                    OnProcessingTask(workTask, taskResult);

                    workTask.InitPreviousResult(previousTaskName, previousTaskResult);
                    workTask.InitParentResult(this);
                    taskResult = taskResult == null ? new TaskResult(true, null) : taskResult;
                    workTask.Initialize(new LoopTaskResult(taskResult, item));
                    previousTaskName = workTask.GetType().FullName;
                    previousTaskResult = workTask.PreviousTaskResult;
                    taskResult = workTask.Execute();
                    workTask.Finish();


                    if (taskResult == null)
                        throw new ArgumentNullException("Task Result is null");

                    if (!taskResult.IsCompleted)
                    {
                        Log("Task Result is not completd. Task Result", null, taskResult);
                        break;
                    }
                    else
                    {
                        OnProcessedTask(workTask, taskResult);
                    }
                }
            }

            taskResult = null;
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

        public ILoopWorkTask<T> UseParallelProcess(int maxDegree)
        {
            _userParallel = true;
            _maxDegree = maxDegree;
            return this;
        }
    }
}
