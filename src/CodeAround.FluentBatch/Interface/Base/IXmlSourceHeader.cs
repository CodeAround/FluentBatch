using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Source;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlSourceHeader
    {
        IXmlSourceNoteType AsHeader();
        IXmlSourceNoteType AsRow();
        IXmlSourceHeader XPath(Func<string> xPath);
    }
}
