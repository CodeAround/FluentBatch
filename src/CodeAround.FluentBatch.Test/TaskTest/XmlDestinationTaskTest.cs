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
using CodeAround.FluentBatch.Interface.Destination;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.Database;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class XmlDestinationTaskTest: IDisposable
    {
        private DatabaseSandBox _sourceDatabase;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public XmlDestinationTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<XmlDestinationTaskTest>();

            _sourceDatabase = new DatabaseSandBox();
            _sourceDatabase.KeepDatabaseAfterTest = false;
            _sourceDatabase.Build(@"(localdb)\Mssqllocaldb", "CodeAroundSouce");
            _sourceDatabase.Migrate();
        }

        [Fact]
        public void Create_xmldestinationTask_with_nodeType_and_createRow()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                                .ToFile(filePath)
                                                .Root("delivery")
                                                .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                                .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                                .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                                .Use(() => "PersonId").IsNode().Parent(() => "Persons")
                                                .Element(() => "data").IsNode().Parent(() => "Persons")
                                                .Element(() => "list").IsNode().Parent(() => "data")
                                                .Element(() => "person").IsNode().Parent(() => "list")
                                                .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                                                 .Add(o => o.Field(() => "Surname").IsNodeField())
                                                                 .Add(o => o.Field(() => "BirthdayDate").IsNodeField())
                                                )
                                                .Build())
                                                .Build();



            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_attribute()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToFile(filePath)
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "PersonId").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                      .Add(o => o.Field(() => "Surname").IsNodeField())
                                      .Add(o => o.Field(() => "BirthdayDate").IsNodeField())
                                  )
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_row_attribute()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToFile(filePath)
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "PersonId").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsAttributeField())
                                      .Add(o => o.Field(() => "Surname").IsAttributeField())
                                      .Add(o => o.Field(() => "BirthdayDate").IsAttributeField())
                                  )
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_nodeType_format_provider()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToFile(filePath)
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "Partner").IsNode().Parent(() => "Persons")
                                  .Use(() => "Name").IsNode().Parent(() => "Persons")
                                  .Use(() => "Surname", () => new CultureInfo("it-IT")).IsNode().Parent(() => "Persons")
                                  .Use(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNode().Parent(() => "Persons")
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_attribute_format_provider()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                                  .ToFile(filePath)
                                                  .Root("delivery")
                                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                                  .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                                  .Use(() => "Name").IsAttribute().Parent(() => "Persons")
                                                  .Use(() => "Surname", () => new CultureInfo("it-IT")).IsAttribute().Parent(() => "Persons")
                                                  .Use(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsAttribute().Parent(() => "Persons")
                                                 .Build())
                                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_row_attribute_format_provider()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToFile(filePath)
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                      .Add(o => o.Field(() => "Surname", () => new CultureInfo("it-IT")).IsNodeField())
                                      .Add(o => o.Field(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNodeField())
                                  )
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);
            };

            flow.Run();
            Assert.True(File.Exists(filePath));

            var file = new FileInfo(filePath);
            Assert.True(file.Length > 0);
            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            file.Delete();
        }

        [Fact]
        public void Create_xmldestinationTask_with_row_attribute_string_source()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToXmlString()
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                      .Add(o => o.Field(() => "Surname", () => new CultureInfo("it-IT")).IsNodeField())
                                      .Add(o => o.Field(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNodeField())
                                  )
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);

                if (e.WorkTask is IXmlDestination)
                {
                    Assert.NotNull(e.CurrentTaskResult.Result);
                    Assert.NotEmpty(e.CurrentTaskResult.Result.ToString());
                }
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmldestinationTask_with_row_attribute_stream_source()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Then(task => task.CreateXmlDestination()
                                  .ToStream()
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                      .Add(o => o.Field(() => "Surname", () => new CultureInfo("it-IT")).IsNodeField())
                                      .Add(o => o.Field(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNodeField())
                                  )
                                  .Build())
                              .Build();

            object result = null;

            flow.ProcessedTask += (s, e) =>
            {
                Assert.True(e.CurrentTaskResult.IsCompleted);

                if (e.WorkTask is IXmlDestination)
                {
                    Assert.NotNull(e.CurrentTaskResult.Result);
                    Assert.NotNull(((Stream)e.CurrentTaskResult.Result).Length > 0);
                }
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmldestinationTask_with_nodeType_and_simple_loop()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            string filePath = Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml_result.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                .Then(task => task.CreateLoop<int>()
                              .AddLoop(new List<int>() { 1, 3, 4 })
                              .ProcessedTaskEvent((s, e) =>
                              {
                                  Assert.True(e.CurrentTaskResult.IsCompleted);
                                  if (e.WorkTask is IXmlDestination)
                                  {
                                      Assert.NotNull(e.CurrentTaskResult.Result);
                                      Assert.True(File.Exists(e.CurrentTaskResult.Result.ToString()));
                                  }
                              })
                              .Append(x => x.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Append(x => x.CreateXmlDestination()
                                                .ToFile(filePath)
                                                .Root("delivery")
                                                .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                                .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                                .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                                .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                                .Element(() => "data").IsNode().Parent(() => "Persons")
                                                .Element(() => "list").IsNode().Parent(() => "data")
                                                .Element(() => "person").IsNode().Parent(() => "list")
                                                .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                                    .Add(o => o.Field(() => "Surname", () => new CultureInfo("it-IT")).IsNodeField())
                                                    .Add(o => o.Field(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNodeField())
                                                )
                                                .Build())
                            .Build())
                .Build();

            flow.Run();

            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");


            DirectoryInfo dir = new DirectoryInfo(assemblyPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }

        [Fact]
        public void Create_xmldestinationTask_with_nodeType_and_file_loop()
        {
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641385', 'George','Best', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641366', 'Paul','Blend', GETDATE())");
            _sourceDatabase.Connection.Execute("INSERT INTO [dbo].[Persons] VALUES('25641377', 'Rick', 'Red', GETDATE())");

            string assemblyPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "TestResult");
            if (!Directory.Exists(assemblyPath))
                Directory.CreateDirectory(assemblyPath);

            var lst = new List<string>();
            lst.Add(Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml1.xml"));
            lst.Add(Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml2.xml"));
            lst.Add(Path.Combine(assemblyPath, $@"{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}_xml3.xml"));

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                .Then(task => task.CreateLoop<string>()
                              .AddLoop(lst)
                              .ProcessedTaskEvent((s, e) =>
                              {
                                  Assert.True(e.CurrentTaskResult.IsCompleted);
                                  if (e.WorkTask is IXmlDestination)
                                  {
                                      Assert.NotNull(e.CurrentTaskResult.Result);
                                      Assert.True(File.Exists(e.CurrentTaskResult.Result.ToString()));
                                  }
                              })
                              .Append(x => x.CreateSqlSource()
                                                        .UseConnection(_sourceDatabase.Connection)
                                                        .FromTable("Persons", "dbo")
                                                        .Build())
                              .Append(x => x.CreateXmlDestination()
                                  .UseLoopValueAsFilename()
                                  .Root("delivery")
                                  .AddNameSpace("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "https://microsoft.com/v1/externalrevenue")
                                  .AddNameSpace("xsi", "schemaLocation", "https://microsoft.com/v1/externalrevenue", "https://microsoft.com/v1/externalrevenue.xsd")
                                  .Element(() => "Persons").IsNode().Parent(() => "delivery")
                                  .Use(() => "Partner").IsAttribute().Parent(() => "Persons")
                                  .Element(() => "data").IsNode().Parent(() => "Persons")
                                  .Element(() => "list").IsNode().Parent(() => "data")
                                  .Element(() => "person").IsNode().Parent(() => "list")
                                  .CreateRow(t => t.Add(y => y.Field(() => "Name").IsNodeField())
                                      .Add(o => o.Field(() => "Surname", () => new CultureInfo("it-IT")).IsNodeField())
                                      .Add(o => o.Field(() => "BirthdayDate", () => new CultureInfo("it-IT")).IsNodeField())
                                  )
                                  .Build())
                              .Build())
                .Build();
            flow.Run();

            _sourceDatabase.Connection.Execute("delete from [dbo].[Persons] ");

            DirectoryInfo dir = new DirectoryInfo(assemblyPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }

        public void Dispose()
        {
            _sourceDatabase.Dispose();
        }
    }
}


