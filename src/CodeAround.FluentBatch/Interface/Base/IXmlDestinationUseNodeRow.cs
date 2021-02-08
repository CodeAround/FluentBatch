using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Interface.Base
{
    public interface IXmlDestinationUseNodeRow
    {
        IXmlDestinationRow IsAttributeField();
        IXmlDestinationRow IsNodeField();
    }
}
