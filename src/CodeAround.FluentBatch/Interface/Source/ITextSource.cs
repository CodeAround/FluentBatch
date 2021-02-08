using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface ITextSource : IFault
    {
        ITextSource From(TextReader reader);

        ITextSource From(string file);

        ITextSource SkipWhen(Func<string, bool> func);

        ITextSource ParseAsDelimited(char delimiter, bool useHeader);

        ITextSource ParseAsDelimited(char[] delimiters, bool useHeader);

        ITextSource ParseAsFixedColumns(int[] colWidths, bool useHeader);

        ITextSource ParseWith(Func<string, IEnumerable<string>> lineSplitter, bool useHeader);

        ITextSource AddOtherField(Func<string> fieldName);
    }
}
