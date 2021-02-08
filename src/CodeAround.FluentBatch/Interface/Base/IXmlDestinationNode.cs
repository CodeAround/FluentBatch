using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Destination;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlDestinationNode
    {
        IXmlDestination Parent(Func<string> parentNode);
        IXmlDestination CreateRow(Func<IXmlDestinationRow, IXmlDestinationRow> row);
    }
}
