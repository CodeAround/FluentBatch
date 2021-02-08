using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlDestinationRow
    {
        IXmlDestinationRow Add(Func<IXmlDestinationUseRow, IXmlDestinationRow> addRow);
    }
}
