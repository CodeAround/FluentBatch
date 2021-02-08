using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Task.Destination;

namespace CodeAround.FluentBatch.Interface
{
    public interface ITextDestination : IFault
    {
        ITextDestination To(TextWriter writer);
        ITextDestination To(string file);
        ITextDestination WithHeader(bool b);
        ITextDestination SetFieldNames(IEnumerable<string> fieldNames);
        ITextDestination WriteAsFixedColumns(int[] columnWidths, string separator);
        ITextDestination WriteAsFixedColumns(int columnWidth, string separator);
        ITextDestination WriteAsDelimited(string separator);
        ITextDestination WriteAs(Func<IEnumerable<TextDestination.RowItem>, string> funcFormatter);

    }
}
