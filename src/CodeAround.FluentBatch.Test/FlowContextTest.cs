using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Event;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Test.Infrastructure;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test
{
    public class FlowContextTest: IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private DatabaseSandBox _targetDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public FlowContextTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<FlowContextTest>();

            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();

            _targetDatabase = new DatabaseSandBox();
            _targetDatabase.KeepDatabaseAfterTest = false;
            _targetDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundTarget");
            _targetDatabase.Migrate();
        }

        [Fact]
        public void add_task_should_be_return_not_null_flow()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("New Task").Then(task =>
                task.CreateSql().Connection(_sourceDatabase.Connection).Build()).Build();
            Assert.NotNull(flow);
        }

        [Fact]
        public void add_task_should_be_return_ok_without_tasl()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("New Task").Then(task => null).Build();
            Assert.NotNull(flow);
        }
        [Fact]
        public void add_task_should_be_return_InvalidOpertionException()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            var exeption = Assert.Throws<InvalidOperationException>(() => builder.Create("New Task").Then(task =>
                task.CreateSql().Connection(_sourceDatabase.Connection).Build(), 2).Build());
            Assert.Equal("Invalid index exception. The 2 index must be less or equal to number of elements (collection count 0)", exeption.Message);
        }
        [Fact]
        public void add_task_should_be_return_task_result()
        {
            string stm = "INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())";
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("New Task")
                .Then(task => task.CreateSql().Connection(_sourceDatabase.Connection).FromStatement(stm).WithStatementType(StatementCommandType.Command).Build())
                .Then(task => task.CreateSql().Connection(_sourceDatabase.Connection).FromStatement("Select * From [dbo].[Persons]").WithStatementType(StatementCommandType.Query)
                    .Build()).Build();
            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                result = e.WorkTask.TaskResult;
            };

            flow.Run();
            Assert.NotNull(result);
        }

        [Fact]
        public void add_task_should_be_null()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("New Task").Then(task =>
              task.CreateSql().Connection(null).Build()));
        }

        [Fact]
        public void run_should_return_not_null_processedTask_result()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("New Task").Then(task =>
                task.CreateSql().Connection(_sourceDatabase.Connection).FromStatement("Select * From [dbo].[Persons]")
                    .WithStatementType(StatementCommandType.Command).Build()).Build();
            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                result = e.CurrentTaskResult;
            };
            flow.Run();
            Assert.NotNull(result);
        }

        [Fact]
        public void run_should_return_not_null_processed_workTask()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("New Task").Then(task =>
                task.CreateSql().Connection(_sourceDatabase.Connection).FromStatement("Select * From [dbo].[Persons]")
                    .WithStatementType(StatementCommandType.Command).Build()).Build();
            object result = null;
            flow.ProcessedTask += (s, e) =>
            {
                result = e.WorkTask;
            };
            flow.Run();
            Assert.NotNull(result);
        }

        [Fact]
        public void flowContext_should_contains_previous_task_result()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<CustomWorkTask>())
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_targetDatabase.Connection)
                                                .Table("Persons")
                                                .Schema("dbo")
                                                .DeleteFirst()
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                             .Build())
                             .Build();
            Dictionary<string, object> result = null;
            flow.ProcessedTask += (s, e) =>
            {
                result = e.WorkTask.PreviousTaskResult;
            };
            flow.Run();
            Assert.True(result.Count == 3);

        }


        public void Dispose()
        {
            _targetDatabase.Dispose();
            _sourceDatabase.Dispose();
        }
    }
}
