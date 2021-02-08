using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class LoopWorkTaskTest: IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public LoopWorkTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<LoopWorkTaskTest>();


            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();
        }

        [Fact]
        public void create_loop_for_insert_shoult_be_return_element()
        {
            List<Persons> persons = new List<Persons>();

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "George";
            person.Surname = "Best";
            person.BirthdayDate = DateTime.Now;

            persons.Add(person);


            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Task with loop")
                .Then(task => task.Name("First Loop").CreateLoop<Persons>().AddLoop(persons)
                .Append(t => t.Create<CustomWorkTask>())
                .Append(t => t.CreateSqlDestination()
                              .UseConnection(_sourceDatabase.Connection)
                               .Table("Persons")
                                .Schema("dbo")
                                .Map(() => "PersonId", () => "PersonId", false)
                                .Map(() => "Name", () => "Name", false)
                                .Map(() => "Surname", () => "Surname", false)
                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                              .TruncteFirst()
                              .Build())
                         .Build()
                ).Build();
            
            flow.Run();
            var result = _sourceDatabase.Connection.Query<int>("SELECT * FROM [dbo].[Persons]");
            Assert.Single(result);
        }

      
        [Fact]
        public void create_loop_return_nullArgumentException_with_parameters_null()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(()=> builder.Create("Task with loop")
                .Then(task => task.CreateLoop<int>().AddLoop(null)
                    .Append(t => t.Create<CustomWorkTask>())
                    .Append(t => t.CreateSqlDestination()
                        .UseConnection(_sourceDatabase.Connection)
                        .Table("Persons")
                                .Schema("dbo")
                                .Map(() => "PersonId", () => "PersonId", false)
                                .Map(() => "Name", () => "Name", false)
                                .Map(() => "Surname", () => "Surname", false)
                                .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                        .TruncteFirst()
                        .Build())
                    .Build()
                ).Build());
        }

        [Fact]
        public void create_loop_return_nullArgumentException_with_not_valid_name()
        {
            List<Persons> persons = new List<Persons>();

            Persons person = new Persons();
            person.PersonId = 25641385;
            person.Name = "George";
            person.Surname = "Best";
            person.BirthdayDate = DateTime.Now;

            persons.Add(person);
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Task with loop")
                                                              .Then(task => task.CreateLoop<Persons>()
                                                                                .AddLoop(persons)
                                                                                .Append(t => t.Create<CustomWorkTask>())
                                                                                .Append(t => t.CreateSqlDestination()
                                                                                            .UseConnection(_sourceDatabase.Connection)
                                                                                            .Table("Persons")
                                                                                            .Schema("dbo")
                                                                                            .Map(() => "PersonId", () => "PersonId", false)
                                                                                            .Map(() => "Name", () => "Name", false)
                                                                                            .Map(() => "Surname", () => "Surname", false)
                                                                                            .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                                                            .TruncteFirst()
                                                                                  .Build())
                                                                .Build())
                                                       .Build());
        }

        public void Dispose()
        {
            _sourceDatabase.Dispose();
        }
    }
}
