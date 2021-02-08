using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeAround.FluentBatch.Infrastructure
{
    public enum RowOperation
    {
        Process,
        Insert,
        Update,
        Delete,
        Ignore
    }

    public enum StatementCommandType
    {
        Query,
        Command
    }

    public enum OperationType
    {
        None,
        Insert,
        Update,
        Delete
    }

    public enum XMLFormatType
    {
        Header,
        Row
    }

    public enum XMLNodeType
    {
        IsAttribute,
        IsNode
    }

    public enum LoopXmlSource
    {
        Filename,
        Xml,
        Resource
    }

    public enum XMLDestinationType
    {
        Filename,
        String,
        Stream
    }

    public enum LoopSource
    {
        Filename,
        String
    }
}

