using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class JsonSourceTaskTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        [Obsolete]
        public JsonSourceTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<JsonSourceTaskTest>();
        }

        [Fact]
        public void Load_json_from_file()
        {
            List<string> fields = new List<string>();
            fields.Add("Id");
            fields.Add("CardOwner");
            fields.Add("Balance");
            fields.Add("BankName");
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string jsonFilePath = Path.Combine(assemblyPath, @"..\..\Infrastructure\JsonTest.json");
            string fileDestinationPath = Path.Combine(assemblyPath, @"..\..\Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Json File")
                              .Then(task => task.Name("EmailTask").CreateJsonSource<CardList>()
                                                .FromFile(jsonFilePath)
                                                .CollectionPropertyName("Cards")
                                                .Map(() => "IdCard", () => "Id")
                                                .Map(() => "CardOwner", () => "CardOwner")
                                                .Map(() => "CardNumber", () => "CardNumber")
                                                .Map(() => "CardExpiryDate", () => "CardExpiryDate")
                                                .Map(() => "CardBalance", () => "Balance")
                                                .Map(() => "BankName", () => "BankName")
                                                .Build())
                               .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To(fileDestinationPath)
                                                .Build())
                              .Build();

            flow.Run();
            var file = new FileInfo(fileDestinationPath);
            Assert.True(file.Length > 0);
            file.Delete();
        }

        [Fact]
        public void Load_json_from_string()
        {
            string json = @"{
  ""Cards"": [
    {
      ""IdCard"": ""{68BFE0B7-A62D-4EEB-990C-E426B7FEB86F}"",
      ""IdCardType"": ""{01D78DD9-612A-4E24-978C-2C21ED26603F}"",
      ""CardOwner"": ""George Best"",
      ""CardNumber"": ""123456789"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{939C16F4-147B-40EA-8757-8E57D0FE384B}"",
      ""BankAccountOwner"": ""George Best"",
      ""BankName"": ""Unicredit""
    },
    {
      ""IdCard"": ""{D6D60A63-BBE7-4BED-AE83-ECBE66BA36E3}"",
      ""IdCardType"": ""{F6C33632-48FC-4AF9-9AA3-A9B68AFFBA7F}"",
      ""CardOwner"": ""Cris Paul"",
      ""CardNumber"": ""78777788888"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{35739379-F8F8-4DBE-B9B3-EFB8FF88C894}"",
      ""BankAccountOwner"": ""Cris Paul"",
      ""BankName"": ""Banco di Napoli""
    },
    {
      ""IdCard"": ""{F42FD52A-A367-4E9A-96B2-EFDF74B16E61}"",
      ""IdCardType"": ""{1348A23A-C144-473A-B4AA-3B485BE67C66}"",
      ""CardOwner"": ""Alex Trest"",
      ""CardNumber"": ""999999999999999"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{D1607A2E-BBFB-4AE2-AE46-58F4E646DEA3}"",
      ""BankAccountOwner"": ""Alex Trest"",
      ""BankName"": ""Poste Italiane""
    }
  ],
  ""Message"": ""string"",
  ""IsCompleted"": true
}";

            List<string> fields = new List<string>();
            fields.Add("Id");
            fields.Add("CardOwner");
            fields.Add("Balance");
            fields.Add("BankName");
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string fileDestinationPath = Path.Combine(assemblyPath, @"..\..\Infrastructure\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Json File")
                              .Then(task => task.Name("EmailTask").CreateJsonSource<CardList>()
                                                .FromString(json)
                                                .CollectionPropertyName("Cards")
                                                .Map(() => "IdCard", () => "Id")
                                                .Map(() => "CardOwner", () => "CardOwner")
                                                .Map(() => "CardNumber", () => "CardNumber")
                                                .Map(() => "CardExpiryDate", () => "CardExpiryDate")
                                                .Map(() => "CardBalance", () => "Balance")
                                                .Map(() => "BankName", () => "BankName")
                                                .Build())
                               .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To(fileDestinationPath)
                                                .Build())
                              .Build();

            flow.Run();
            var file = new FileInfo(fileDestinationPath);
            Assert.True(file.Length > 0);
            file.Delete();
        }

        [Fact]
        public void Load_json_from_file_in_a_loop()
        {
            List<string> files = new List<string>();

            List<string> fields = new List<string>();
            fields.Add("Id");
            fields.Add("CardOwner");
            fields.Add("Balance");
            fields.Add("BankName");
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            files.Add(Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\JsonTest.json"));
            files.Add(Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\JsonTest2.json"));

            string fileDestinationPath = Path.Combine(assemblyPath, @"TestResult\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Json File")
                              .Then(task => task.Name("loop").CreateLoop<string>()
                                               .AddLoop(files)
                                               .Append(x => x.Name("EmailTask").CreateJsonSource<CardList>()
                                                            .LoopBehaviour(FluentBatch.Infrastructure.LoopSource.Filename)
                                                            .CollectionPropertyName("Cards")
                                                            .Map(() => "IdCard", () => "Id")
                                                            .Map(() => "CardOwner", () => "CardOwner")
                                                            .Map(() => "CardNumber", () => "CardNumber")
                                                            .Map(() => "CardExpiryDate", () => "CardExpiryDate")
                                                            .Map(() => "CardBalance", () => "Balance")
                                                            .Map(() => "BankName", () => "BankName")
                                                            .Build())
                                               .Append(x => x.CreateTextDestination()
                                                                .WithHeader(true)
                                                                .SetFieldNames(fields)
                                                                .WriteAsDelimited(",")
                                                                .To(fileDestinationPath)
                                                                .Build())
                                               .ProcessedTaskEvent((s, e) =>
                                               {
                                                   Assert.True(e.CurrentTaskResult.IsCompleted);
                                                   Assert.True(((IEnumerable<IRow>)e.CurrentTaskResult.Result).Count() > 0);
                                               })
                                               .Build())
                              .Build();
            flow.Run();

        }

        [Fact]
        public void Load_json_from_string_in_a_loop()
        {
            List<string> values = new List<string>();

            List<string> fields = new List<string>();
            fields.Add("Id");
            fields.Add("CardOwner");
            fields.Add("Balance");
            fields.Add("BankName");
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            values.Add(@"{
  ""Cards"": [
    {
      ""IdCard"": ""{68BFE0B7-A62D-4EEB-990C-E426B7FEB86F}"",
      ""IdCardType"": ""{01D78DD9-612A-4E24-978C-2C21ED26603F}"",
      ""CardOwner"": ""George Best"",
      ""CardNumber"": ""123456789"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{939C16F4-147B-40EA-8757-8E57D0FE384B}"",
      ""BankAccountOwner"": ""George Best"",
      ""BankName"": ""Unicredit""
    },
    {
      ""IdCard"": ""{D6D60A63-BBE7-4BED-AE83-ECBE66BA36E3}"",
      ""IdCardType"": ""{F6C33632-48FC-4AF9-9AA3-A9B68AFFBA7F}"",
      ""CardOwner"": ""Cris Paul"",
      ""CardNumber"": ""78777788888"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{35739379-F8F8-4DBE-B9B3-EFB8FF88C894}"",
      ""BankAccountOwner"": ""Cris Paul"",
      ""BankName"": ""Banco di Napoli""
    },
    {
      ""IdCard"": ""{F42FD52A-A367-4E9A-96B2-EFDF74B16E61}"",
      ""IdCardType"": ""{1348A23A-C144-473A-B4AA-3B485BE67C66}"",
      ""CardOwner"": ""Alex Trest"",
      ""CardNumber"": ""999999999999999"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{D1607A2E-BBFB-4AE2-AE46-58F4E646DEA3}"",
      ""BankAccountOwner"": ""Alex Trest"",
      ""BankName"": ""Poste Italiane""
    }
  ],
  ""Message"": ""string"",
  ""IsCompleted"": true
}");
            values.Add(@"{
  ""Cards"": [
    {
      ""IdCard"": ""{68BFE0B7-A62D-4EEB-990C-E426B7FEB86F}"",
      ""IdCardType"": ""{01D78DD9-612A-4E24-978C-2C21ED26603F}"",
      ""CardOwner"": ""George Best"",
      ""CardNumber"": ""123456789"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{939C16F4-147B-40EA-8757-8E57D0FE384B}"",
      ""BankAccountOwner"": ""George Best"",
      ""BankName"": ""Unicredit""
    },
    {
      ""IdCard"": ""{D6D60A63-BBE7-4BED-AE83-ECBE66BA36E3}"",
      ""IdCardType"": ""{F6C33632-48FC-4AF9-9AA3-A9B68AFFBA7F}"",
      ""CardOwner"": ""Cris Paul"",
      ""CardNumber"": ""78777788888"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{35739379-F8F8-4DBE-B9B3-EFB8FF88C894}"",
      ""BankAccountOwner"": ""Cris Paul"",
      ""BankName"": ""Banco di Napoli""
    },
    {
      ""IdCard"": ""{F42FD52A-A367-4E9A-96B2-EFDF74B16E61}"",
      ""IdCardType"": ""{1348A23A-C144-473A-B4AA-3B485BE67C66}"",
      ""CardOwner"": ""Alex Trest"",
      ""CardNumber"": ""999999999999999"",
      ""CardExpiryDate"": ""2019-08-05T10:06:36.077Z"",
      ""CardBalance"": 0,
      ""Currency"": ""EUR"",
      ""IdBankAccounts"": ""{D1607A2E-BBFB-4AE2-AE46-58F4E646DEA3}"",
      ""BankAccountOwner"": ""Alex Trest"",
      ""BankName"": ""Poste Italiane""
    }
  ],
  ""Message"": ""string"",
  ""IsCompleted"": true
}");

            string fileDestinationPath = Path.Combine(assemblyPath, @"TestResult\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Json File")
                              .Then(task => task.Name("loop").CreateLoop<string>()
                                               .AddLoop(values)
                                               .Append(x => x.Name("EmailTask").CreateJsonSource<CardList>()
                                                            .LoopBehaviour(FluentBatch.Infrastructure.LoopSource.String)
                                                            .CollectionPropertyName("Cards")
                                                            .Map(() => "IdCard", () => "Id")
                                                            .Map(() => "CardOwner", () => "CardOwner")
                                                            .Map(() => "CardNumber", () => "CardNumber")
                                                            .Map(() => "CardExpiryDate", () => "CardExpiryDate")
                                                            .Map(() => "CardBalance", () => "Balance")
                                                            .Map(() => "BankName", () => "BankName")
                                                            .Build())
                                               .Append(x => x.CreateTextDestination()
                                                                .WithHeader(true)
                                                                .SetFieldNames(fields)
                                                                .WriteAsDelimited(",")
                                                                .To(fileDestinationPath)
                                                                .Build())
                                               .ProcessedTaskEvent((s, e) =>
                                               {
                                                   Assert.True(e.CurrentTaskResult.IsCompleted);
                                                   Assert.True(((IEnumerable<IRow>)e.CurrentTaskResult.Result).Count() > 0);
                                               })
                                               .Build())
                              .Build();
            flow.Run();

        }

        [Fact]
        public void Load_json_from_file_fault()
        {
            List<string> fields = new List<string>();
            fields.Add("Id");
            fields.Add("CardOwner");
            fields.Add("Balance");
            fields.Add("BankName");
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string jsonFilePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\JsonTest.json");
            string fileDestinationPath = Path.Combine(assemblyPath, @"TestResult\fileDestinationExample.csv");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("Json File")
                              .Then(task => task.Name("EmailTask").CreateJsonSource<CardList>()
                                                .FromFile(jsonFilePath)
                                                .CollectionPropertyName("Wrongs")
                                                .Map(() => "IdCard", () => "Id")
                                                .Map(() => "CardOwner", () => "CardOwner")
                                                .Map(() => "CardNumber", () => "CardNumber")
                                                .Map(() => "CardExpiryDate", () => "CardExpiryDate")
                                                .Map(() => "CardBalance", () => "Balance")
                                                .Map(() => "BankName", () => "BankName")
                                                .Fault(x =>
                                                {
                                                    Assert.NotNull(x.CurrentException);
                                                })
                                                .Build())
                               .Then(task => task.CreateTextDestination()
                                                .WithHeader(true)
                                                .SetFieldNames(fields)
                                                .WriteAsDelimited(",")
                                                .To(fileDestinationPath)
                                                .Build())
                              .Build();

            flow.Run();

        }
    }
}
