using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlDestinationUseRow
    {
        IXmlDestinationUseNodeRow Field(Func<string> node);
        IXmlDestinationUseNodeRow Field(Func<string> node, Func<IFormatProvider> provider);

    }
}
