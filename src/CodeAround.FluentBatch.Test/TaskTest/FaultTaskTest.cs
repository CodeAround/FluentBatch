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

namespace CodeAround.FluentBatch.Test.TaskTest
{
    public class FaultTaskTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        [Obsolete]
        public FaultTaskTest()
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            ILoggerFactory factory = new LoggerFactory().AddNLog();
            _logger = factory.CreateLogger<FaultTaskTest>();
        }

        [Fact]
        public void textSourceFault()
        {
            string xml = @"<delivery xmlns:xsi=""https://microsoft.com/v1/externalrevenue"" xsi:schemaLocation=""https://microsoft.com/v1/externalrevenue.xsd""> 
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
		                       </transactions>";

            
            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                .Then(task => task.CreateXmlSource()
                    .FromString(xml)
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
                    .Fault(x =>
                    {
                        Assert.True(x != null);
                    })
                    .Build())
                .Build();

            flow.Run();
        }

        [Fact]
        public void textSourceFault_wit_loop()
        {
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
		                       </transactions>");

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
		                       </transactions>");

            FlowBuilder builder = new FlowBuilder(_logger);
            var flow = builder.Create("First")
                .Then(task => task.CreateLoop<string>()
                    .AddLoop(_xml)
                    .Append(x => x.CreateXmlSource()
                        .LoopBehaviour(LoopXmlSource.Xml)
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
                        .Fault(y =>
                        {
                            Assert.True(y != null);
                        })
                        .Build())
                    .Build())
                .Build();

            flow.Run();
        }
    }
}
