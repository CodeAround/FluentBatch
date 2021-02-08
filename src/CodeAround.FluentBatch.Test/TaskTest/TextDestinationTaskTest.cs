using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Task.Destination;
using CodeAround.FluentBatch.Test.Infrastructure;
using Xunit;
using CodeAround.FluentBatch.Test.TestUtilities;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
   public class TextDestinationTaskTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        public TextDestinationTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<TextDestinationTaskTest>();
        }

        [Fact]
        public void textDestination_write_to_file()
        {
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            fields.Add("BirthdayDate");
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            string fileDestinationPath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)
                              .Build())
                              .Then(task=> task.Create<TextCustomWorkTask>())
                              .Then(task=> task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To(fileDestinationPath)
                              .Build())
                              .Build();

            
            flow.Run();
            var file = new FileInfo(fileDestinationPath).Length;
            Assert.True(file > 0);
        }

        [Fact]
        public void textDestination_should_return_argumentNullExecption_with_not_valid_string_path()
        {
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            fields.Add("BirthdayDate");
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(()=> builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)
                              .Build())
                              .Then(task => task.Create<TextCustomWorkTask>())
                              .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To("")
                            .Build())
                            .Build());
        }


        [Fact]
        public void textDestination_should_return_argumentNullExecption_with_not_valid_text_writer()
        {
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            fields.Add("BirthdayDate");
            char delimeter = ',';
            TextWriter wrt = null;
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                              .Then(task => task.CreateTextSource()
                                                                                .From(filePath)
                                                                                .ParseAsDelimited(delimeter, true)
                                                              .Build())
                                                              .Then(task => task.Create<TextCustomWorkTask>())
                                                              .Then(task => task.CreateTextDestination()
                                                                                .WithHeader(true)
                                                                                .SetFieldNames(fields)
                                                                                .WriteAsDelimited(",")
                                                                                .To(wrt)
                                                              .Build())
                                                              .Build());
        }

        [Fact]
        public void textDestination_write_with_fixed_columns()
        {
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            fields.Add("BirthdayDate");
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            string fileDestinationPath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)
                              .Build())
                              .Then(task => task.Create<TextCustomWorkTask>())
                              .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsFixedColumns(16, ",")
                                                .To(fileDestinationPath)
                              .Build())
                              .Build();


            flow.Run();
            var file = new FileInfo(fileDestinationPath).Length;
            Assert.True(file > 0);
        }

        
        [Fact]
        public void textDestination_write_with_multiple_fixed_columns()
        {
            int[] fixColumn = new[] { 8, 5, 10, 10 };
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            fields.Add("BirthdayDate");
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            string fileDestinationPath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)
                                                .Build())
                                .Then(task => task.Create<TextCustomWorkTask>())
                                .Then(task => task.CreateTextDestination()
                                                  .WithHeader(true)
                                                  .SetFieldNames(fields)
                                                  .WriteAsFixedColumns(fixColumn, ",")
                                                 .To(fileDestinationPath)
                                .Build())
                                .Build();


            flow.Run();
            var file = new FileInfo(fileDestinationPath).Length;
            Assert.True(file > 0);
        }

        [Fact]
        public void textDestination_write_with_fields()
        {
            List<string> fields = new List<string>();
            fields.Add("PersonId");
            fields.Add("Name");
            fields.Add("Surname");
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            string fileDestinationPath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)
                              .Build())
                              .Then(task => task.Create<TextCustomWorkTask>())
                              .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To(fileDestinationPath)
                                                .Build())
                               .Build();


            flow.Run();
            var file = new FileInfo(fileDestinationPath).Length;
            Assert.True(file > 0);
        }
    }
}
