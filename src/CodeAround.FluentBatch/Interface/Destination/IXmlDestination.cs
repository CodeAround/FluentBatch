using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Destination
{
    public interface IXmlDestination : IXmlDestinationNode, IXmlDestinationNoteType, IFault, IXmlDestinationUseRow, IXmlDestinationUseNodeRow, IXmlDestinationRow
    {
        IXmlDestination ToFile(string file);
        IXmlDestination Root(string root);
        IXmlDestination AddNameSpace(string prefix, string localName, string ns, string value);
        IXmlDestinationNoteType Use(Func<string> node);
        IXmlDestinationNoteType Use(Func<string> node, Func<IFormatProvider> provider);
        IXmlDestination ToXmlString();
        IXmlDestination ToStream();
        IXmlDestination UseLoopValueAsFilename();
        IXmlDestination Element(Func<string> nodeElement);

    }
}
