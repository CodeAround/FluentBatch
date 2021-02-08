using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Task;

namespace CodeAround.FluentBatch.Interface.Builder
{
    public interface IBuilder
    {
        IWorkTask Build();
    }
}
