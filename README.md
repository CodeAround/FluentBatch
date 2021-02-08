# Codearound.FluentBatch

[![NuGet](https://img.shields.io/badge/nuget-v1.0.3-blue)](https://www.nuget.org/packages/CodeAround.FluentBatch/) ![Code Coverage](https://img.shields.io/badge/Code%20Coverage-92%25-green) ![Supported Platform](https://img.shields.io/badge/Supported%20Platform-.net%20Standard%202.0-yellow) ![Test Number](https://img.shields.io/badge/Test%20Number-%23109-red)

The library allows to create complex batch / workflow using fluent syntax. The library is .Net Standard 2.0 and included more common work tasks like Database source and destination, text source and destination, xml source and destination, loop work task, conditional work task, Excel source, json source. 

Otherwise you can implements a custom work task easly and at the end you can develop a extended module for the engine

## Setup

you can download the package from Nuget : [CodeAround.FluentBatch](https://www.nuget.org/packages/CodeAround.FluentBatch/) 

## Description

Fluent Batch is an engine to create a batch based on fluent syntax

Main features are
- .Net Standard 2.0 (.Net core ready)
- Compatibility with all relational database (powered by [Dapper](https://www.nuget.org/packages/Dapper/))
- Create you custom work task for each kind of custom implementation
- Implements a Microsoft.Extension.Logging to allow choosing of common log library ([NLog](https://www.nuget.org/packages/Nlog/), [Serilog](https://www.nuget.org/packages/Serilog/) etc)
- Creating a extension of engine to included new type of work task
- Available +12 several work task type

## Work Task Type

The engine support 3 kind of work task
1) Source Tasks
2) Generic or Base Tasks
3) Destination Tasks

### Source Tasks
The source task contains all tasks that grab anything information and transform these to proprietary structure used in the later tasks. In few words these task represents a **Input Task** type
The default source tasks are:

- [Excel Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Excel-Source): Allow to grab the information from a Excel file (powered by [ExcelDataReader](https://www.nuget.org/packages/ExcelDataReader/))
- [Json Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Json-Source): Allow to grab the information from a Json (powered by [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/))
- [Object Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Object-Source): Allow to grab the information from a .net object
- [Sql Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Sql-Source): Allow to grab the information from any type of relational database (powered by [Dapper](https://www.nuget.org/packages/Dapper/))
- [Text Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Text-Source): Allow to grab the information from any type of Text file included csv and fixed lenght file
- [Xml Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Xml-Source): Allow to grab the information from Xml

### Destination Tasks
The destination task contains all tasks that wirte anything of informations received in input. These are a **Output Task** type
The default destination tasks are:

- [Sql Destination](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Sql-Destination): Allow to persist some information in any type of relational database any type of relational database (powered by [Dapper](https://www.nuget.org/packages/Dapper/))
- [Text Destination](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Text-Destination): Allow to persist some information in text file included csv and fixed lenght file
- [Xml Destination](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Xml-Destination): Allow to persist some information in xml file

### Generic or Base Tasks
There are a kind of tasks that are used to enrich the power of engine, in particular you can create add a loop behavior or a conditions behaviour of your flow.

the generic tasks are:

- [Loop Work Task](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Loop-Task): Allow to implement a loop behavior in your workflow
- [Condition Wor Task](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Condition-Task): Allow to implement a condition behavior in your workflow
- [Sql Work Task](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Sql-Task): Allow to implement a common database work task to execute db command (powered by [Dapper](https://www.nuget.org/packages/Dapper/))


## How to use

This is an example to create a batch that grab data from excel and put into sql server database.

The excel has one sheet with this information

|PersonId|Name|Surname|Age|
| ------------------- | :------------------: | ------------------- | :------------------: |
|1|George|Best|23|
|2|Paul|Blend|44|

The code to implement this task is :

```csharp

  var builder = new FlowBuilder(_logger);
  var flow = builder.Create("Excel Source")
                    .Then(task => task.Name("ExcelWorkTask")
                                      .CreateExcelSource()
                                      .FromFile("[PUT_YOUR_FILE_PATH]")
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
                                       .UseConnection([PUT_YOUR_CONNECTION_STRING])
                                       .Table("PersonDetail")
                                       .Schema("dbo")
                                       .Map(() => "PersonId", () => "PersonId", true)
                                       .Map(() => "Name", () => "Name", false)
                                       .Map(() => "Surname", () => "Surname", false)
                                       .Specify(() => "BirthdayDate", () => DateTime.Now)
                     .Build())
                    .Build();

  flow.Run();
```

First of all I create a [FlowBuilder](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Flow-Builder) instance passing an instance of [Microsoft.Extension.Logging](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Logging) then :
- Create a [Excel Source](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Excel-Source) Task, specifying the excel file path, if there is an header and the map of fields 
- Create a custom task called **InsertCustomExcelTask** that mark each transformed rows as to Insert

```csharp

public override TaskResult Execute()
{
    if (_rows != null && _rows.Count() > 0)
    {
        foreach (var row in _rows)
        {
            row.Operation = OperationType.Insert;
        }
    }
    return new TaskResult(true, _rows);
}

```
- Create a [Sql Destination](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Sql-Destination) Task specifying a destination table, schema, connection string and the mapping of fields

you can found a complete code in the test called excelSource_should_retun_completed_status_with_header_in_sql_destination in the ExcelSourceTest.cs test file

## Create Custom Task

Go [Create Custom Task](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Create-New-Custom-Task) Wiki page

## Extensions

FluentBatch allow to create an extension of engine to create a specific and reusable task type in easly mode. 

Now 2 extensions are available:
- [Codearound.FluentBatch.Email](https://www.nuget.org/packages/Codearound.FluentBatch.Email/) that allow to send an email in your flow (Powered by [FluentEmail](https://www.nuget.org/packages/FluentEmail.Core/)) 
- [Codearound.FluentBatch.SqlScript](https://www.nuget.org/packages/Codearound.FluentBatch.SqlScript/) that allow to create sql script file as work tast destination

Go [Create New Extension](https://github.com/CodearoundHub/Codearound.FluentBatch/wiki/Create-New-Extension) Wiki page
