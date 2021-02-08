using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Task;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface IXmlSource : IXmlSourceHeader, IXmlSourceNoteType, IFault
    {
        IXmlSource FromFile(string file);
        IXmlSource FromString(string xmlSource);
        IXmlSource FromResource(string resourceFilename);
        IXmlSourceHeader Use(Func<string> node);
        IXmlSource Root(string root);
        IXmlSource AddNameSpace(string prefix, string uri);
        IXmlSource RootRow(string rootRow);
        IXmlSource IdentifyRow(string identifyRow);
        IXmlSource LoopBehaviour(LoopXmlSource xmlSource);
    }
}
