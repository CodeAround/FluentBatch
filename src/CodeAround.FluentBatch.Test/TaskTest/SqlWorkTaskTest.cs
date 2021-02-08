using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class SqlWorkTaskTest: IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public SqlWorkTaskTest()
        {
            var executingFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyFolder = new DirectoryInfo(".").FullName;
            

            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<SqlWorkTaskTest>();

            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();
        }

        [Fact]
        public void sqlWork_task_read_file_and_execute_query()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlWorktask")
                              .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .FromFile(filePath).Build())
                               .Build();

            flow.Run();

            var result = _sourceDatabase.Connection.Query<int>("SELECT * FROM [dbo].[Persons]");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void sqlWorktask_should_be_return_ArgumentNullException_with_wrong_file_path()
        {
            string filePath = "";
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlWorktask")
                                                              .Then(task => task.CreateSql()
                                                                                .Connection(_sourceDatabase.Connection)
                                                                                .FromFile(filePath).Build())
                                                               .Build());
        }

        [Fact]
        public void sqlWorkTask_fromStatement_with_parameter()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            int personId = 25641385;
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlWorktask")
                .Then(task => task.CreateSql()
                                  .Connection(_sourceDatabase.Connection)
                                  .FromFile(filePath).Build(), 0)
                 .Then(task => task.CreateSql().Connection(_sourceDatabase.Connection)
                                               .AddParameter("PersonId", personId)
                                               .FromStatement("SELECT * FROM [dbo].[Persons] WHERE PersonId = @PersonId")
                                               .WithStatementType(StatementCommandType.Query)
                                               .Build(), 1)
                .Build();
            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void sqlWorkTask_connection_string_empty()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");

            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlWorktask")
                                                              .Then(task => task.CreateSql()
                                                                                .Connection(null)
                                                                                .FromFile(filePath).Build(), 0)
                                                              .Build());
        }

        [Fact]
        public void sqlWorkTask_Sqlconnection_null()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            SqlConnection connection = null;
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlWorktask")
                .Then(task => task.CreateSql()
                    .Connection(connection)
                    .FromFile(filePath).Build(), 0)
                .Build());
        }

        [Fact]
        public void sqlWorkTask_fromResource_with_parameter()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            int personId = 25641385;
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlWorktask")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .FromFile(filePath).Build(), 0)
                .Then(task => task.CreateSql().Connection(_sourceDatabase.Connection)
                    .AddParameter("PersonId", personId)
                    .FromResource("Infrastructure.ResourceSql.sql", Assembly.GetExecutingAssembly())
                    .WithStatementType(StatementCommandType.Query)
                    .Fault(x =>
                     {
                        Assert.True(x == null);
                     })
                    .Build(), 1)
                .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }


        [Fact]
        public void sqlWorkTask_fromResource_with_parameter_and_connection()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            int personId = 25641385;
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlWorktask")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .FromFile(filePath).Build(), 0)
                .Then(task => task.CreateSql().Connection(_sourceDatabase.Connection)
                    .AddParameter("PersonId", personId)
                    .FromResource("Infrastructure.ResourceSql.sql", Assembly.GetExecutingAssembly())
                    .WithStatementType(StatementCommandType.Query)
                    .Fault(x =>
                    {
                        Assert.True(x == null);
                    })
                    .Build(), 1)
                .Build();
            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void sqlWorktask_using_loopvalue_for_parameter()
        {
            List<string>  personList = new List<string>();
            personList.Add("25641385");

            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            var builder = new  FlowBuilder(_logger);
            var flow = builder.Create("LoopValue")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .FromFile(filePath)
                    .Build())
                .Then(task => task.CreateLoop<string>()
                    .AddLoop(personList)
                    .Append(t => t.CreateSql()
                        .FromStatement(
                            "Delete from [dbo].[Persons] Where PersonId =@personList")
                        .Connection(_sourceDatabase.Connection)
                        .WithStatementType(StatementCommandType.Command)
                        .UseLoopValue(true, "personList")
                        .Build())
                    .Build())
                .Build();

            flow.Run();

            var res = _sourceDatabase.Connection.Query("Select * from [dbo].[Persons]").ToList(); 
            Assert.True(res.Count == 0);
        }

        public void Dispose()
        {
            _sourceDatabase.Dispose();
        }
    }
}
