using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class ObjectSourceTaskTest : IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public ObjectSourceTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<ObjectSourceTaskTest>();


            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();
        }

        [Fact]
        public void create_object_source_task_and_save_in_SqlDestination()
        {
            var persons = new List<Persons>()
            {
                new Persons() {
                    PersonId = 25641385,
                    Name = "George", 
                    Surname = "Best", 
                    BirthdayDate = DateTime.Now 
                },
                new Persons() {
                    PersonId = 999,
                    Name = "Paul",
                    Surname = "Grey",
                    BirthdayDate = DateTime.Now
                }
            };

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("ObjectSourceTask")
                               .Then(task => task.CreateObjectSource()
                                                .From(persons).Build())
                               .Then(task => task.Create<CustomWorkTask>())
                               .Then(task => task.CreateSqlDestination()
                                                  .UseConnection(_sourceDatabase.Connection)
                                                  .Table("Persons")
                                                    .Schema("dbo")
                                                    .Map(() => "PersonId", () => "PersonId", false)
                                                    .Map(() => "Name", () => "Name", false)
                                                    .Map(() => "Surname", () => "Surname", false)
                                                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                  .TruncteFirst()
                               .Build())

                    .Build();
            flow.Run();
            var result = _sourceDatabase.Connection.Query<Persons>("SELECT * FROM [dbo].[Persons]");
            Assert.NotEmpty(result);
        }

        [Fact]
        public void create_object_source_should_return_argumentNullexecption()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("ObjectSourceTask")
                                                              .Then(task => task.CreateObjectSource()
                                                              .From(null).Build())
                                                              .Then(task => task.Create<CustomWorkTask>())
                                                              .Then(task => task.CreateSqlDestination()
                                                                                .UseConnection(_sourceDatabase.Connection)
                                                                                .Table("Persons")
                                                                                    .Schema("dbo")
                                                                                    .Map(() => "PersonId", () => "PersonId", false)
                                                                                    .Map(() => "Name", () => "Name", false)
                                                                                    .Map(() => "Surname", () => "Surname", false)
                                                                                    .Map(() => "BirthdayDate", () => "BirthdayDate", false)
                                                                                  .TruncteFirst()
                                                                        .Build())
                                                               .Build());

        }

        public void Dispose()
        {
            _sourceDatabase.Dispose();
        }
    }
}
