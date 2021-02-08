using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Source;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlSourceNoteType
    {
        IXmlSource IsAttribute();
        IXmlSource IsNode();
    }
}
