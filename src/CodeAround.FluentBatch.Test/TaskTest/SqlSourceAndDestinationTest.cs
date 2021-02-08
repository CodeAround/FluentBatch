using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Test.Database;
using CodeAround.FluentBatch.Test.Infrastructure;
using Dapper;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class SqlSourceAndDestinationTest : IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private DatabaseSandBox _targetDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;
        private readonly ITestOutputHelper _output;

        public SqlSourceAndDestinationTest(ITestOutputHelper output)
        {
            _output = output;
            var executingFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _output.WriteLine(executingFolder);
            string assemblyFolder = new DirectoryInfo(".").FullName;

            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<SqlSourceAndDestinationTest>();

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
        public void sqlSource_test_fromQuery()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger, true);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromQuery("SELECT * FROM dbo.[Persons]")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                 .Schema("dbo")
                                                 .Map(() => "PersonId", () => "PersonId", false)
                                                 .Map(() => "Name", () => "Name", false)
                                                 .Map(() => "Surname", () => "Surname", false)
                                                 .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);
        }

        [Fact]
        public void sqlSource_test_fromQuery_commandTimeout()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger, true);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromQuery("SELECT * FROM dbo.[Persons]")
                                                 .WithCommandTimeout(1)
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                 .Schema("dbo")
                                                 .Map(() => "PersonId", () => "PersonId", false)
                                                 .Map(() => "Name", () => "Name", false)
                                                 .Map(() => "Surname", () => "Surname", false)
                                                 .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);
        }

        public void Dispose()
        {
            _targetDatabase.Dispose();
            _sourceDatabase.Dispose();
        }

        [Fact]
        public void sqlSource_should_be_return_nullArgumentException_with_empty_Connection()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlSourcetask")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .WithStatementType(StatementCommandType.Command)
                    .FromFile(filePath)
                    .Build())
                .Then(task => task.CreateSqlSource()
                    .UseConnection(null)
                    .FromQuery("SELECT  * FROM dbo.[Persons]")
                    .Build())
                .Then(task => task.Create<CustomWorkTask>())
                .Then(task => task.CreateSqlDestination()
                    .UseConnection(_targetDatabase.Connection)
                    .Table("PersonDetail")
                    .Schema("dbo")
                    .Map(() => "PersonId", () => "PersonId", false)
                    .Map(() => "Name", () => "Name", false)
                    .Map(() => "Surname", () => "Surname", false)
                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                    .Build())
                .Build());

        }

        [Fact]
        public void sqlSource_test_fromQuery_should_be_return_nullArgumentException_with_empty_query()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromQuery("")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                               .Build())
                        .Build());
        }

        [Fact]
        public void sqlSource_test_fromTable()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromTable("Persons", "dbo")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .DeleteFirst()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlSource_test_fromTable_should_be_return_nullArgumentException_with_empty_tableName()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromTable("", "")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                               .Build())
                        .Build());
        }

        [Fact]
        public void sqlSource_test_fromStoredProcedure_should_be_return_nullArgumentException_with_empty_tableName()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("SqlSourcetask")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .WithStatementType(StatementCommandType.Command)
                    .FromFile(filePath)
                    .Build())
                .Then(task => task.CreateSqlSource()
                    .UseConnection(_sourceDatabase.Connection)
                    .FromStoredProcedure("")
                    .Build())
                .Then(task => task.Create<CustomWorkTask>())
                .Then(task => task.CreateSqlDestination()
                    .UseConnection(_targetDatabase.Connection)
                   .Table("PersonDetail")
                    .Schema("dbo")
                    .Map(() => "PersonId", () => "PersonId", false)
                    .Map(() => "Name", () => "Name", false)
                    .Map(() => "Surname", () => "Surname", false)
                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                    .Build())
                .Build());
        }

        [Fact]
        public void sqlSource_test_fromStoredProcedure()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            string sqlPath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\TestStoredProcedure.sql");
            string personId = "25641385";
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSql()
                                    .Connection(_sourceDatabase.Connection)
                                    .WithStatementType(StatementCommandType.Command)
                                    .FromFile(sqlPath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromStoredProcedure("GivePersons_By_personID")
                                                 .AddParameter("PersonId", personId)

                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                .TruncteFirst()
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlSource_test_fromQuery_withParameters()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            
            string personId = "25641385";
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromQuery("SELECT * FROM dbo.Persons WHERE PersonId = @PersonId AND Name = @Name")
                                                 .AddParameter("PersonId", personId)
                                                 .AddParameter("Name", "George")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                .TruncteFirst()
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlDestiantion_test_setDeleteFirst()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromTable("Persons", "dbo")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                .DeleteFirst()
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlDestiantion_test_setIdentityInsert()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromTable("Persons", "dbo")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonIdentity")
                                                    .Schema("dbo")
                                                    .Map(() => "PersonId", () => "PersonId", false)
                                                    .Map(() => "Name", () => "Name", false)
                                                    .Map(() => "Surname", () => "Surname", false)
                                                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                 .Specify(() => "ID", () => 10)
                                                 .IdentityInsert()
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonIdentity]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlDestiantion_test_setIdentityInsert_commandTimeout()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                               .Then(task => task.CreateSql()
                                                .Connection(_sourceDatabase.Connection)
                                                .WithStatementType(StatementCommandType.Command)
                                                .FromFile(filePath)
                               .Build())
                               .Then(task => task.CreateSqlSource()
                                                 .UseConnection(_sourceDatabase.Connection)
                                                 .FromTable("Persons", "dbo")
                               .Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                 .WithCommandTimeout(30)
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonIdentity")
                                                    .Schema("dbo")
                                                    .Map(() => "PersonId", () => "PersonId", false)
                                                    .Map(() => "Name", () => "Name", false)
                                                    .Map(() => "Surname", () => "Surname", false)
                                                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                 .Specify(() => "ID", () => 10)
                                                 .IdentityInsert()
                               .Build())
                        .Build();
            flow.Run();
            var result = _targetDatabase.Connection.Query<int>("SELECT * FROM [dbo].[PersonIdentity]");
            Assert.NotEmpty(result);

        }

        [Fact]
        public void sqlDestination_should_be_return_update_row()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                 .WithStatementType(StatementCommandType.Command)
                                                 .Connection(_sourceDatabase.Connection)
                                                 .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<UpdateCustomWorkTask>()
                                                .AddParameter("Name", "Paul"))
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .Table("Persons")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                              .Build())
                          .Build();
            flow.Run();
            var result = _sourceDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[Persons]");
            Assert.Equal("Paul", result.FirstOrDefault().Name);
        }

        [Fact]
        public void sqlDestination_should_be_return_update_row_with_updateall()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                 .WithStatementType(StatementCommandType.Command)
                                                 .Connection(_sourceDatabase.Connection)
                                                 .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<UpdateCustomWorkTask>()
                                                .AddParameter("Name", "Paul"))
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .Table("Persons")
                                                .Schema("dbo")
                                                .UpdateAllFields()
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                              .Build())
                          .Build();
            flow.Run();
            var result = _sourceDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[Persons]");
            Assert.Equal("Paul", result.FirstOrDefault().Name);
        }

        [Fact]
        public void sqlDestination_should_be_return_update_row_with_updateall_false()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                 .WithStatementType(StatementCommandType.Command)
                                                 .Connection(_sourceDatabase.Connection)
                                                 .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<UpdateCustomWorkTask>()
                                                .AddParameter("Name", "Paul"))
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .Table("Persons")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                              .Build())
                          .Build();
            flow.Run();
            var result = _sourceDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[Persons]");
            Assert.Equal("Paul", result.FirstOrDefault().Name);
        }

        [Fact]
        public void sqlDestination_should_delete_row()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTest.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                .Then(task => task.CreateSql()
                    .Connection(_sourceDatabase.Connection)
                    .WithStatementType(StatementCommandType.Command)
                    .FromFile(filePath)
                    .Build())
                .Then(task => task.CreateSqlSource()
                    .UseConnection(_sourceDatabase.Connection)
                    .FromTable("Persons", "dbo")
                    .Build())
                .Then(task => task.Create<UpdateCustomWorkTask>())
                .Then(task => task.CreateSqlDestination()
                    .UseConnection(_sourceDatabase.Connection)
                    .Table("PersonDetail")
                    .Schema("dbo")
                    .Map(() => "PersonId", () => "PersonId", false)
                    .Map(() => "Name", () => "Name", false)
                    .Map(() => "Surname", () => "Surname", false)
                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                    .Build())
                .Build();
            flow.Run();
            var result = _sourceDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.True(!result.Any());
        }

        [Fact]
        public void sqlDestination_should_insert_with_transaction_failed()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTestTransaction.sql");
            FlowBuilder builder = new FlowBuilder(_logger, true);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                 .WithStatementType(StatementCommandType.Command)
                                                 .Connection(_sourceDatabase.Connection)
                                                 .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<InsertWithTransactionCustomTask>())
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_targetDatabase.Connection)
                                                .UseTransaction()
                                                .DeleteFirst()
                                                .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                .Fault((x) =>
                                                {
                                                    Assert.NotNull(x);
                                                })
                                                .Build())
                          .Build();

            _targetDatabase.Connection.Execute("delete FROM [dbo].[PersonDetail]");

            flow.Run();
            var result = _targetDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[PersonDetail]");
            Assert.True(result.Count() == 0);
        }

        [Fact]
        public void sqlDestination_should_insert_with_transaction()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\queryTestTransaction.sql");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("SqlSourcetask")
                              .Then(task => task.CreateSql()
                                                 .WithStatementType(StatementCommandType.Command)
                                                 .Connection(_sourceDatabase.Connection)
                                                 .FromFile(filePath)
                              .Build())
                              .Then(task => task.CreateSqlSource()
                                                .UseConnection(_sourceDatabase.Connection)
                                                .FromTable("Persons", "dbo")
                              .Build())
                              .Then(task => task.Create<InsertCustomTask>())
                              .Then(task => task.CreateSqlDestination()
                                                .UseConnection(_targetDatabase.Connection)
                                                .UseTransaction()
                                                .DeleteFirst()
                                                .Table("PersonDetail")
                                                .Schema("dbo")
                                                .Map(() => "PersonId", () => "PersonId", false)
                                                .Map(() => "Name", () => "Name", false)
                                                .Map(() => "Surname", () => "Surname", false)
                                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                .Build())
                          .Build();

            _targetDatabase.Connection.Execute("delete FROM [dbo].[PersonDetail]");

            flow.Run();
            var result = _targetDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[PersonDetail]");
            
            Assert.True(result.Count() == 3);
        }
    }
}
