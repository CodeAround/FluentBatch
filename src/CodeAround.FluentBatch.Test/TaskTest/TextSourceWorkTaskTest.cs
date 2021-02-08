using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CodeAround.FluentBatch.Engine;
using Xunit;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class TextSourceWorkTaskTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        public TextSourceWorkTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<TextSourceWorkTaskTest>();
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_file_path()
        {
            string filePath = "";
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                .Then(task => task.CreateTextSource().From(filePath).Build()).Build());
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_textReader()
        {
            TextReader textReader = null;
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                .Then(task => task.CreateTextSource().From(textReader).Build()).Build());
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_delimiter()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                              .Then(task => task.CreateTextSource()
                                                                                .From(filePath)
                                                                                .ParseAsDelimited(null,false)
                                                                                .Build()).Build());
        }


        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_white_space_delimiter()
        {
            char delimiter = ' ';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                              .Then(task => task.CreateTextSource()
                                                                                .From(filePath)
                                                                                .ParseAsDelimited(delimiter, false)
                                                                                .Build()).Build());
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_colWidths()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                              .Then(task => task.CreateTextSource()
                                                                    .From(filePath)
                                                                    .ParseAsFixedColumns(null, false)
                                                                    .Build()).Build());
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_parseWith()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                              .Then(task => task.CreateTextSource()
                                                                    .From(filePath)
                                                                    .ParseWith(null,false)
                                                                    .Build()).Build());
        }

        [Fact]
        public void TestSource_should_be_return_nullArgumentException_with_not_valid_func_skip_when()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("Text Source Work Task")
                                                               .Then(task => task.CreateTextSource()
                                                                    .From(filePath)
                                                                    .SkipWhen(null)
                                                                    .Build()).Build());
        }


        [Fact]
        public void textSource_should_read_file_and_return_result()
        {
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter,true)
                              .Build())
                              .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void TestSource_should_return_rows_with_textReader()
        {
            char[] delimeters = {','};
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");

            using (TextReader reader = File.OpenText(filePath))
            {
                FlowBuilder builder = new FlowBuilder(_logger);
                var flow = builder.Create("Text Source Work Task")
                                  .Then(task => task.CreateTextSource()
                                                    .From(reader)
                                                    .ParseAsDelimited(delimeters, true)
                                  .Build())
                                  .Build();

                flow.ProcessedTask += (s, e) =>
                {
                    Assert.NotNull(e.CurrentTaskResult);
                };

                flow.Run();
            }
        }

        [Fact]
        public void textSource_should_return_invalid_data_exception_with_empty_file()
        {
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\EmptyFile.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsDelimited(delimeter, true)  
                                                .Fault(x =>
                                                {
                                                    Assert.True(x != null);
                                                })
                              .Build())
                              .Build();


            flow.Run();
        }

        [Fact]
        public void textSource_should_return_invalid_operation_exception_with_empty_file()
        {
            char delimeter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\EmptyFile.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .ParseAsDelimited(delimeter, true)
                                                .Fault(x =>
                                                {
                                                    Assert.True(x != null);
                                                })
                              .Build())
                              .Build();

            flow.Run();
        }

        [Fact]
        public void textSource_should_read_file_with_coma_line_split()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseWith( line => line.Split(','), true)
                             .Build())
                            .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void textSource_should_read_file_with_fixed_columns()
        {
            int[] fixColumn = new[] {8,5,10,10};
            
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\testFile.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                              .Then(task => task.CreateTextSource()
                                                .From(filePath)
                                                .ParseAsFixedColumns(fixColumn,false)
                              .Build())
                              .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void textSource_should_read_file_with_skip_row()
        {
            char delimiter = ',';
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\fileExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                               .Then(task => task.CreateTextSource()
                                                 .From(filePath)
                                                 .ParseAsDelimited(delimiter,true)
                                                 .SkipWhen(line => line.StartsWith(" "))
                               .Build())
                               .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void textSource_add_others_field()
        {
            int[] fixColumn = new[] { 8, 5, 10, 10, 4 };
            
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\testFile.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Text Source Work Task")
                               .Then(task => task.CreateTextSource()
                                                 .From(filePath)
                                                 .ParseAsFixedColumns(fixColumn, false)
                                                 .AddOtherField(() => "IsValid")
                               .Build())
                               .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }
    }
}
