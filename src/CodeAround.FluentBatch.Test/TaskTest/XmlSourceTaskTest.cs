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
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using Xunit;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class XmlSourceTaskTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        public XmlSourceTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<XmlSourceTaskTest>();
        }
        
        [Fact]
        public void Create_xmlsourceTask_with_nodeType()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateXmlSource()
                                                .FromFile(filePath)
                                                .Use(() => "channel_id").AsHeader().IsNode()
                                                .Use(() => "creation_date").AsHeader().IsNode()
                                                .Use(() => "creation_time").AsHeader().IsNode()
                                                .Use(() => "transaction").AsRow().IsNode()
                                                .Use(() => "id").AsRow().IsNode()
                                                .Use(() => "loyalty_id").AsRow().IsNode()
                                                .AddNameSpace("xmlns:xsi","https://microsoft.com/v1/internalrevenuerec")
                                                .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/externalrevenue.xsd")
                                                .IdentifyRow("transaction")
                                                .RootRow("transactions")
                                                .Root("delivery")
                                                .Build())
                            .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmlsourceTask_with_header_as_attribute()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec2.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                               .Then(task => task.CreateXmlSource()
                                                 .FromFile(filePath)
                                                 .Use(() => "channel_id").AsHeader().IsAttribute()
                                                 .Use(() => "creation_date").AsHeader().IsAttribute()
                                                 .Use(() => "creation_time").AsHeader().IsAttribute()
                                                 .Use(() => "transaction").AsRow().IsNode()
                                                 .Use(() => "id").AsRow().IsNode()
                                                 .Use(() => "loyalty_id").AsRow().IsNode()
                                                 .AddNameSpace("xmlns:xsi", "https://microsoft.com/v1/internalrevenuerec")
                                                 .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/externalrevenue.xsd")
                                                 .IdentifyRow("transaction")
                                                 .RootRow("transactions")
                                .Build())
                                .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmlsourceTask_with_rows_as_attribute()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec3.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                               .Then(task => task.CreateXmlSource()
                                                  .FromFile(filePath)
                                                  .Use(() => "channel_id").AsHeader().IsAttribute()
                                                  .Use(() => "creation_date").AsHeader().IsAttribute()
                                                  .Use(() => "creation_time").AsHeader().IsAttribute()
                                                  .Use(() => "id").AsRow().IsAttribute()
                                                  .Use(() => "loyalty_id").AsRow().IsAttribute()
                                                  .AddNameSpace("xmlns:xsi", "https://microsoft.com/v1/internalrevenuerec")
                                                  .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/externalrevenue.xsd")
                                                  .IdentifyRow("transaction")
                                                  .RootRow("transactions")
                                .Build())
                                .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmlsourceTask_filename_with_in_loop()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            
            List<string> _files = new List<string>();
            _files.Add(Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\interface-10n-externalrevenuerec.xml"));
            _files.Add(Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\interface-10n-externalrevenuerec2.xml"));
            _files.Add(Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty), @"Infrastructure\interface-10n-externalrevenuerec3.xml"));

            
            var builder = new FlowBuilder(_logger);

            var flow = builder.Create("Process Ack Loader")
                .Then(task => task.CreateLoop<string>()
                    .ProcessedTaskEvent((s,e) =>
                    {
                        Assert.True(e.CurrentTaskResult != null && ((IEnumerable<IRow>)e.CurrentTaskResult).Any() );
                    })
                    .AddLoop(_files)
                    .Append(t => t.CreateXmlSource()
                        .IdentifyRow("transaction")
                        .LoopBehaviour(LoopXmlSource.Filename)
                        .RootRow("transactions")
                        .Root("delivery")
                        .Use(() => "creation_date").AsHeader().IsNode()
                        .Use(() => "creation_time").AsHeader().IsNode()
                        .Use(() => "sender_id").AsHeader().IsNode()
                        .Use(() => "receiver_id").AsHeader().IsNode()
                        .Use(() => "id").AsRow().IsNode()
                        .Use(() => "loyalty_id").AsRow().IsNode()
                        .Use(() => "billable_points").AsRow().IsNode()
                        .Use(() => "confirmed").AsRow().IsNode()
                        .AddNameSpace("xmlns:xsi", "https://microsoft.com/v1/internalrevenuerec")
                        .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/internalrevenuerec.xsd")
                        .Build())
                    .Build()).Build();

            flow.Run();
        }

        [Fact]
        public void Create_xmlsourceTask_xml_with_in_loop()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            List<string> _xml = new List<string>();
            _xml.Add(@"<delivery xmlns:xsi=""https://microsoft.com/v1/externalrevenue"" xsi:schemaLocation=""https://microsoft.com/v1/externalrevenue.xsd""> 
                              <channel_id>1</channel_id>
                              <creation_date>30/04/2019</creation_date>
                              <creation_time>2019-04-30T16:12:00Z</creation_time>
                              <transactions>		
			                  <transaction>
				                <id>22863a1d-6bfd-4cb7-bee3-db58b05527bd</id>
				                <loyalty_id>3234823762</loyalty_id>
				                <billable_points>50</billable_points>
				                <confirmed>true</confirmed>
			                  </transaction>
			                  <transaction>
				                <id>34563a1d-6bfd-4cb7-bee3-db58b0552234</id>
				                <loyalty_id>3346724866</loyalty_id>
				                <billable_points>0</billable_points>
				                <confirmed>false</confirmed>
			                   </transaction>
		                       </transactions>
                               </delivery>");
            _xml.Add(@"<delivery xmlns:xsi=""https://microsoft.com/v1/externalrevenue"" xsi:schemaLocation=""https://microsoft.com/v1/externalrevenue.xsd""> 
                              <channel_id>1</channel_id>
                              <creation_date>30/04/2019</creation_date>
                              <creation_time>2019-04-30T16:12:00Z</creation_time>
                              <transactions>		
			                  <transaction>
				                <id>22863a1d-6bfd-4cb7-bee3-db58b05527bd</id>
				                <loyalty_id>3234823762</loyalty_id>
				                <billable_points>50</billable_points> 
				                <confirmed>true</confirmed>
			                  </transaction>
			                  <transaction>
				                <id>34563a1d-6bfd-4cb7-bee3-db58b0552234</id>
				                <loyalty_id>3346724866</loyalty_id>
				                <billable_points>0</billable_points> 
				                <confirmed>false</confirmed>
			                   </transaction>
		                       </transactions>
                               </delivery>");


            var builder = new FlowBuilder(_logger);

            var flow = builder.Create("Process Ack Loader")
                .Then(task => task.CreateLoop<string>()
                    .ProcessedTaskEvent((s, e) =>
                    {
                        Assert.True(e.CurrentTaskResult != null && ((IEnumerable<IRow>)e.CurrentTaskResult).Any());
                    })
                    .AddLoop(_xml)
                    .Append(t => t.CreateXmlSource()
                        .IdentifyRow("transaction")
                        .LoopBehaviour(LoopXmlSource.Xml)
                        .RootRow("transactions")
                        .Root("delivery")
                        .Use(() => "creation_date").AsHeader().IsNode()
                        .Use(() => "creation_time").AsHeader().IsNode()
                        .Use(() => "sender_id").AsHeader().IsNode()
                        .Use(() => "receiver_id").AsHeader().IsNode()
                        .Use(() => "id").AsRow().IsNode()
                        .Use(() => "loyalty_id").AsRow().IsNode()
                        .Use(() => "billable_points").AsRow().IsNode()
                        .Use(() => "confirmed").AsRow().IsNode()
                        .AddNameSpace("xmlns:xsi", "https://microsoft.com/v1/internalrevenuerec")
                        .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/internalrevenuerec.xsd")
                        .Build())
                    .Build()).Build();

            flow.Run();
        }

        [Fact]
        public void create_xmlSource_with_string_input_xml()
        {
            string input = @"<delivery xmlns:xsi=""https://microsoft.com/v1/externalrevenue"" xsi:schemaLocation=""https://microsoft.com/v1/externalrevenue.xsd""> 
                              <channel_id>1</channel_id>
                              <creation_date>30/04/2019</creation_date>
                              <creation_time>2019-04-30T16:12:00Z</creation_time>
                              <transactions>		
			                  <transaction>
				                <id>22863a1d-6bfd-4cb7-bee3-db58b05527bd</id>
				                <loyalty_id>3234823762</loyalty_id>
				                <billable_points>50</billable_points> 
				                <confirmed>true</confirmed>
			                  </transaction>
			                  <transaction>
				                <id>34563a1d-6bfd-4cb7-bee3-db58b0552234</id>
				                <loyalty_id>3346724866</loyalty_id>
				                <billable_points>0</billable_points> 
				                <confirmed>false</confirmed>
			                   </transaction>
		                       </transactions>
                               </delivery>";
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                              .Then(task => task.CreateXmlSource()
                                                 .FromString(input)
                                                 .Use(() => "channel_id").AsHeader().IsNode()
                                                 .Use(() => "creation_date").AsHeader().IsNode()
                                                 .Use(() => "creation_time").AsHeader().IsNode()
                                                 .Use(() => "transaction").AsRow().IsNode()
                                                 .Use(() => "id").AsRow().IsNode()
                                                 .Use(() => "loyalty_id").AsRow().IsNode()
                                                 .AddNameSpace("xmlns:xsi", "https://microsoft.com/v1/internalrevenuerec")
                                                 .AddNameSpace("xsi:schemaLocation", "https://microsoft.com/v1/externalrevenue.xsd")
                                                 .IdentifyRow("transaction")
                                                 .RootRow("transactions")
                                                 .Root("delivery")
                                .Build())
                                .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void Create_xmlsourceTask_with_nodeType_as_xpath()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\xpath_test.xml");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                .Then(task => task.CreateXmlSource()
                    .FromFile(filePath)
                    .Use(() => "channel_id").AsHeader().IsNode()
                    .Use(() => "creation_date").AsHeader().IsNode()
                    .Use(() => "creation_time").AsHeader().IsNode()
                    .Use(() => "number_of_credits").XPath(() => "delivery/data/overview/number_of_credits").AsHeader().IsNode()
                    .Use(() => "promo_credit_number").AsRow().IsNode()
                    .Use(() => "loyalty_id").AsRow().IsNode()
                    .AddNameSpace("xmlns:xsi", "https://viseca.ch/manor/v1/promocredit")
                    .AddNameSpace("xsi:schemaLocation", "https://viseca.ch/manor/v1/promocredit.xsd")
                    .IdentifyRow("promo_credit")
                    .RootRow("data")
                    .Fault(x =>
                    {
                        Assert.True(x == null);
                    })
                    .Build())
                .Build();

            flow.ProcessedTask += (s, e) =>
            {
                Assert.NotNull(e.CurrentTaskResult);
            };

            flow.Run();
        }

        [Fact]
        public void createSourceXml_shoul_return_argumentNullException_with_not_valid_inputFile()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                .Then(task => task.CreateXmlSource()
                    .FromFile("")
                    .Build())
                .Build());
        }

        [Fact]
        public void createSourceXml_shoul_return_argumentNullException_with_not_not_valdid_nodeName()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                .Then(task => task.CreateXmlSource()
                    .FromFile(filePath)
                    .Use(null).AsHeader().IsAttribute()
                    .Build())
                .Build());
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_nameSpace()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromFile(filePath)
                                                                                .AddNameSpace("","")
                                                                                .Build())
                                                             .Build());
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_IdentifyRow()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromFile(filePath)
                                                                                .IdentifyRow("")
                                                             .Build())
                                                             .Build());
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_RootRow()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromFile(filePath)
                                                                                .RootRow("")
                                                               .Build())
                                                               .Build());
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_Root()
        {
            string assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string filePath = Path.Combine(assemblyPath.Replace("bin\\Debug", string.Empty),
                @"Infrastructure\interface-10n-externalrevenuerec.xml");
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromFile(filePath)
                                                                                .Root("")
                                                              .Build())
                                                              .Build());
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_FileResource()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromResource(null)
                                                                                .Build())
                                                            .Build());
            
        }

        [Fact]
        public void createSourceXml_should_return_argumentNullException_with_not_not_valdid_inputString()
        {
            FlowBuilder builder = new FlowBuilder(_logger);
            Assert.Throws<ArgumentNullException>(() => builder.Create("First")
                                                              .Then(task => task.CreateXmlSource()
                                                                                .FromString(null)
                                                                                .Build())
                                                                .Build());

        }
    }
}


