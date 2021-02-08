using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using Xunit;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using CodeAround.FluentBatch.Test.Infrastructure;
using CodeAround.FluentBatch.Test.Database;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class ExcelSourceTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;
        private DatabaseSandBox _targetDatabase;

        public ExcelSourceTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<ExcelSourceTest>();

            _targetDatabase = new DatabaseSandBox();
            _targetDatabase.KeepDatabaseAfterTest = false;
            _targetDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundTarget");
            _targetDatabase.Migrate();
        }

        [Fact]
        public void excelSource_should_retun_completed_status_with_header()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");

            var builder = new FlowBuilder(_logger);
            var flow = builder.Create("Excel Source").Then(task => task.Name("ExcelWorkTask")
                                                                                .CreateExcelSource()
                                                                                .FromFile(filePath)
                                                                                .UseHeader(true)
                                                                                .Use(() => "PersonId")
                                                                                .Use(() => "Name")
                                                                                .Use(() => "Surname")
                                                                                .Use(() => "Age")
                                                                                .Build()
                )
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();

        }

        [Fact]
        public void excelSource_should_retun_completed_status_with_header_in_sql_destination()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");

            var builder = new FlowBuilder(_logger);
            var flow = builder.Create("Excel Source")
                              .Then(task => task.Name("ExcelWorkTask")
                                                .CreateExcelSource()
                                                .FromFile(filePath)
                                                .UseHeader(true)
                                                .Use(() => "PersonId")
                                                .Use(() => "Name")
                                                .Use(() => "Surname")
                                                .Use(() => "Age")
                                                .Build()
                                                )
                              .Then(task => task.Name("insert rows").Create<InsertCustomExcelTask>()
                                            .Build())
                               .Then(task => task.CreateSqlDestination()
                                                 .UseConnection(_targetDatabase.Connection)
                                                 .Table("PersonDetail")
                                                 .Schema("dbo")
                                                 .Map(() => "PersonId", () => "PersonId", true)
                                                 .Map(() => "Name", () => "Name", false)
                                                 .Map(() => "Surname", () => "Surname", false)
                                                 .Specify(() => "BirthdayDate", () => DateTime.Now)
                               .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();

        }

        [Fact]
        public void excelSource_should_be_return_argument_null_exception()
        {
            var builder = new FlowBuilder(_logger);
         Assert.Throws<ArgumentNullException>(()=> builder.Create("Excel Source").Then(task => task.CreateExcelSource()
                                                                                .FromFile(null)
                                                                                .UseHeader(false)
                                                                                .Use(() => "Kunden")
                                                                                .Use(() => "Personal")
                                                                                .Use(() => "Legende")
                                                                                .Use(() => "Sprache")
                                                                                .Use(() => "Selektiv")
                                                                                .Build())
                .Build());
        }

        [Fact]
        public void excelSource_should_retun_completed_status_without_header()
        {
            object result = null ;
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");

            var builder = new FlowBuilder(_logger);
            var flow  = builder.Create("Excel Source").Then(task => task.CreateExcelSource()
                                                                                .FromFile(filePath)
                                                                                .UseHeader(true)
                                                                                .Fault(x =>
                                                                                {
                                                                                   Assert.Equal(x.CurrentException.Message, $"Value cannot be null.\r\nParameter name: No column name has been set" );
                                                                                })
                                                                                .Build())
                                .Build();
          
            flow.Run();

        }

        [Fact]
        public void excelSource_should_retun_completed_status_without_column ()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelWithoutHeader.xlsx");

            var builder = new FlowBuilder(_logger);
            var flow = builder.Create("Excel Source").Then(task => task.CreateExcelSource()
                                                                                .FromFile(filePath)
                                                                                .UseHeader(false)
                                                                                .Build())
                .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();

        }

        [Fact]
        public void excelSource_should_return_argument_null_exception_with_not_valid_column_name()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelWithoutHeader.xlsx");

            var builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Excel Source")
                                                                      .Then(task => task.CreateExcelSource()
                                                                                                .FromFile(filePath)
                                                                                                .UseHeader(true)
                                                                                                .Use(null)
                                                                                                .Build())
                                                                      .Build());
        }

        [Fact]
        public void excelSource_should_return_iSCompleted_with_sheet_name()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");

            var builder = new FlowBuilder(_logger);
            var flow = builder.Create("Excel Source").Then(task => task.CreateExcelSource()
                                                                               .FromFile(filePath)
                                                                               .UseHeader(true)
                                                                               .Use(() => "PersonId")
                                                                               .Use(() => "Name")
                                                                               .Use(() => "Surname")
                                                                               .Use(() => "Age")
                                                                               .Sheet("First")
                                                    .Build())
                                                    .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
        }

        [Fact]
        public void excelSource_should_return_iSCompleted_loopWorkTask()
        {
            List<string> listFile = new List<string>();
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");

            listFile.Add(filePath);

            var builder = new FlowBuilder(_logger);

            var flow = builder.Create("Loop")
                .Then(task => task.CreateLoop<string>()
                                          .AddLoop(listFile)
                                          .Append(t => t.CreateExcelSource()
                                                                .FromFile(filePath)
                                                                .UseHeader(true)
                                                                .Use(() => "PersonId")
                                                                .Use(() => "Name")
                                                                .Use(() => "Surname")
                                                                .Use(() => "Age")
                                                                .Sheet("First")
                                                                .Build())
                                          .Build())
                .Build();
            
            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
        }


        [Fact]
        public void excelSource_should_return_argument_null_exception_with_not_valid_name_sheet()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\FileExcelExample.xlsx");
            var builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Excel Source")
                                                                      .Then(task => task.CreateExcelSource()
                                                                                                .FromFile(filePath)
                                                                                                .UseHeader(true)
                                                                                                .Use(() => "PersonId")
                                                                                                .Use(() => "Name")
                                                                                                .Use(() => "Surname")
                                                                                                .Use(() => "Age")
                                                                                                .Sheet(null)
                                                                    .Build())
                                                                    .Build());

        }
    }
}
