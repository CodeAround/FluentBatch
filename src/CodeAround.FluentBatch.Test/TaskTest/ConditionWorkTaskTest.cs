using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class ConditionWorkTaskTest: IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public ConditionWorkTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<ConditionWorkTaskTest>();

            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();
        }

        [Fact]
        public void condition_worktask_then_success()
        {
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "George";
            person.Surname = "Best";
            person.BirthdayDate = DateTime.Now;

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("ConditionWorkTaskTest")
                .Then(task => task.CreateCondition<Persons>()
                                          .ValidationSource(person)
                                          .If((x) => x.Name == "George")
                                          .Then(x => x.CreateSql()
                                                        .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George', 'Best', GETDATE())")
                                                        .Connection(_sourceDatabase.Connection)
                                                        .WithStatementType(StatementCommandType.Command)
                                                        .Build())
                                          .Else(x => x.CreateSql()
                                              .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'Paul', 'Blend', GETDATE())")
                                              .Connection(_sourceDatabase.Connection)
                                              .WithStatementType(StatementCommandType.Command)
                                              .Build())
                                          .Build())
                .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<string>("SELECT Name FROM [dbo].[Persons]").FirstOrDefault();
            Assert.True(result == "George");
        }

        [Fact]
        public void condition_worktask_else_success()
        {
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "Paul";
            person.Surname = "Blend";
            person.BirthdayDate = DateTime.Now;

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("ConditionWorkTaskTest")
                .Then(task => task.CreateCondition<Persons>()
                                          .ValidationSource(person)
                                          .If((x) => x.Name == "George")
                                          .Then(x => x.CreateSql()
                                                        .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())")
                                                        .Connection(_sourceDatabase.Connection)
                                                        .WithStatementType(StatementCommandType.Command)
                                                        .Build())
                                          .Else(x => x.CreateSql()
                                              .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'Paul','Blend', GETDATE())")
                                              .Connection(_sourceDatabase.Connection)
                                              .WithStatementType(StatementCommandType.Command)
                                              .Build())
                                          .Build())
                .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<string>("SELECT Name FROM [dbo].[Persons]").FirstOrDefault();
            Assert.True(result == "Paul");
        }

        [Fact]
        public void condition_worktask_using_loop()
        {
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            List<Persons> persons = new List<Persons>();

            persons.Add(new Persons()
            {
                PersonId = 25641385,
                Name = "Paul",
                Surname = "Blend",
                BirthdayDate = DateTime.Now
            });

            persons.Add(new Persons()
            {
                PersonId = 99641385,
                Name = "George",
                Surname = "Best",
                BirthdayDate = DateTime.Now
            });

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("LoopConditionWorkTaskTest")
                .Then(task => task.Name("PersonList").CreateLoop<Persons>()
                                          .AddLoop(persons)
                                          .Append(y => y.CreateCondition<Persons>()
                                              .UsePreviousTaskResult()
                                              .If((x) => x.Name == "George")
                                              .Then(x => x.CreateSql()
                                                  .FromStatement("INSERT INTO [dbo].[Persons] VALUES('99641385', 'George', 'Best', GETDATE())")
                                                  .Connection(_sourceDatabase.Connection)
                                                  .WithStatementType(StatementCommandType.Command)
                                                  .Build())
                                              .Else(x => x.CreateSql()
                                                  .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'Paul','Blend', GETDATE())")
                                                  .Connection(_sourceDatabase.Connection)
                                                  .WithStatementType(StatementCommandType.Command)
                                                  .Build())
                                              .Build())
                                          .Build())
                .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<string>("SELECT Name FROM [dbo].[Persons] order by Name").ToList();
            Assert.True(result.FirstOrDefault() == "George");
            Assert.True(result.LastOrDefault() == "Paul");
        }

        [Fact]
        public void condition_worktask_using_source()
        {
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                .Then(task => task.CreateTextSource()
                    .From(filePath)
                    .ParseAsDelimited(delimeter, true)
                    .Build())
                .Then(y => y.CreateCondition<IEnumerable<IRow>>()
                    .UsePreviousTaskResult()
                    .If((x) => x.FirstOrDefault()["Name"].ToString() == "George")
                        .Then(x => x.CreateSql()
                        .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())")
                        .Connection(_sourceDatabase.Connection)
                        .WithStatementType(StatementCommandType.Command)
                        .Build())
                    .Else(x => x.CreateSql()
                        .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641386', 'Paul','Blend', GETDATE())")
                        .Connection(_sourceDatabase.Connection)
                        .WithStatementType(StatementCommandType.Command)
                        .Build())
                    .Build())
                .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<string>("SELECT Name FROM [dbo].[Persons] order by Name").ToList();
            Assert.True(result.FirstOrDefault() == "George");
        }

        [Fact]
        public void condition_worktask_process_event()
        {
            string georgeStmt = "INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())";
            string paulStmt = "INSERT INTO [dbo].[Persons] VALUES('25641386', 'Paul','Blend', GETDATE())";
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "Paul";
            person.Surname = "Blend";
            person.BirthdayDate = DateTime.Now;


            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("ConditionWorkTaskTest")
                .Then(task => task.CreateCondition<Persons>()
                                          .ValidationSource(person)
                                          .ProcessingTaskEvent((s, e) =>
                                          {
                                              var stmt = GetPrivateField(e.WorkTask, "_statement");
                                              Assert.True(stmt == paulStmt);
                                          })
                                          .ProcessedTaskEvent((s, e) =>
                                          {
                                              var stmt = GetPrivateField(e.WorkTask, "_statement");
                                              Assert.True(stmt == paulStmt);
                                              Assert.True((int)e.CurrentTaskResult.Result == 1);
                                          })
                                          .If((x) => x.Name == "Paul")
                                          .Then(x => x.CreateSql()
                                                        .FromStatement(paulStmt)
                                                        .Connection(_sourceDatabase.Connection)
                                                        .WithStatementType(StatementCommandType.Command)
                                                        .Build())
                                          .Else(x => x.CreateSql()
                                              .FromStatement(georgeStmt)
                                              .Connection(_sourceDatabase.Connection)
                                              .WithStatementType(StatementCommandType.Command)
                                              .Build())
                                          .Build())
                .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<string>("SELECT Name FROM [dbo].[Persons]").FirstOrDefault();
            Assert.True(result == "Paul");
        }

        [Fact]
        public void condition_worktask_task_result_exception()
        {
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            List<Persons> persons = new List<Persons>();

            persons.Add(new Persons()
            {
                PersonId = 25641385,
                Name = "Paul",
                Surname = "Blend",
                BirthdayDate = DateTime.Now
            });

            persons.Add(new Persons()
            {
                PersonId = 99641385,
                Name = "George",
                Surname = "Best",
                BirthdayDate = DateTime.Now
            });

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("LoopConditionWorkTaskTest")
                .Then(task => task.Name("PersonsList").CreateLoop<Persons>()
                                          .AddLoop(persons)
                                          .Append(y => y.CreateCondition<Persons>()
                                              .UsePreviousTaskResult()
                                              .If((x) => x.Name == "George")
                                              .Then(x => x.CreateSql()
                                                  .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())")
                                                  .Connection(_sourceDatabase.Connection)
                                                  .WithStatementType(StatementCommandType.Command)
                                                  .Build())
                                              .Else(x => x.CreateSql()
                                                  .FromStatement("INSERT INTO [dbo].[Persons] VALUES('25641386', 'Paul','Blend', GETDATE())")
                                                  .Connection(_sourceDatabase.Connection)
                                                  .WithStatementType(StatementCommandType.Command)
                                                  .Build())
                                              .Fault(x =>
                                              {
                                                  Assert.True(x != null);
                                              })
                                              .Build())
                                          .Build())
                .Build();

            flow.Run();

        }

        [Fact]
        public void condition_worktask_processed_event_null()
        {
            string georgeStmt = "INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())";
            string paulStmt = "INSERT INTO [dbo].[Persons] VALUES('25641386', 'Paul','Blend', GETDATE())";
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "Paul";
            person.Surname = "Blend";
            person.BirthdayDate = DateTime.Now;

            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("ConditionWorkTaskTest")
                .Then(task => task.CreateCondition<Persons>()
                                          .ValidationSource(person)
                                          .ProcessedTaskEvent(null)
                                          .If((x) => x.Name == "Paul")
                                          .Then(x => x.CreateSql()
                                                        .FromStatement(georgeStmt)
                                                        .Connection(_sourceDatabase.Connection)
                                                        .WithStatementType(StatementCommandType.Command)
                                                        .Build())
                                          .Else(x => x.CreateSql()
                                              .FromStatement(paulStmt)
                                              .Connection(_sourceDatabase.Connection)
                                              .WithStatementType(StatementCommandType.Command)
                                              .Build())
                                          .Build())
                .Build());

        }

        [Fact]
        public void condition_worktask_processing_event_null()
        {
            string georgeStmt = "INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())";
            string paulStmt = "INSERT INTO [dbo].[Persons] VALUES('25641386', 'Paul','Blend', GETDATE())";
            _sourceDatabase.Connection.Execute("DELETE FROM [dbo].[Persons]");

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "Paul";
            person.Surname = "Blend";
            person.BirthdayDate = DateTime.Now;

            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("ConditionWorkTaskTest")
                .Then(task => task.CreateCondition<Persons>()
                                          .ValidationSource(person)
                                          .ProcessingTaskEvent(null)
                                          .If((x) => x.Name == "George")
                                          .Then(x => x.CreateSql()
                                                        .FromStatement(georgeStmt)
                                                        .Connection(_sourceDatabase.Connection)
                                                        .WithStatementType(StatementCommandType.Command)
                                                        .Build())
                                          .Else(x => x.CreateSql()
                                              .FromStatement(paulStmt)
                                              .Connection(_sourceDatabase.Connection)
                                              .WithStatementType(StatementCommandType.Command)
                                              .Build())
                                          .Build())
                .Build());

        }

        private string GetPrivateField(IWorkTask task, string fieldName)
        {
            var field = task.GetType().GetField(fieldName, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var res = field.GetValue(task);
                return res.ToString();
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _sourceDatabase.Dispose();
        }
    }
}
