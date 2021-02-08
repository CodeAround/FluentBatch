using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface IObjectSource : IFault
    {
        IObjectSource From<T>(IEnumerable<T> dataSource);

        IObjectSource From(IEnumerable dataSource);
    }
}
