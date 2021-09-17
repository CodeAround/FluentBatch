using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Interface
{
    public interface IFlowContext
    {
        int TaskCount { get; }
        void Run();
    }
}
