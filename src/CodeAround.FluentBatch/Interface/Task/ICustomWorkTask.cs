using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Task
{
    public interface ICustomWorkTask : IWorkTask, IFault
    {
        ICustomWorkTask AddParameter(string name, object value);
    }
}
