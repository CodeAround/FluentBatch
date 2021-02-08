using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface IExcelSource : IFault
    {
        IExcelSource FromFile(string fileInput);
        IExcelSource UseHeader(bool usingHeader);
        IExcelSource Use(Func<string> columnName);
        IExcelSource Sheet(string name);


    }
}
