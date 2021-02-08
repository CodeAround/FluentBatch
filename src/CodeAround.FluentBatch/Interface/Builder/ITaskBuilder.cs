using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Destination;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Task.Source;

namespace CodeAround.FluentBatch.Interface.Builder
{
    public interface ITaskBuilder : IWorkTaskBuilder, IExtensionBehaviour
    {
        ITextSource CreateTextSource();
        ITaskBuilder Name(string name);

        IObjectSource CreateObjectSource();

        ISqlSource CreateSqlSource();

        ISqlDestination CreateSqlDestination();

        IXmlSource CreateXmlSource();

        IXmlDestination CreateXmlDestination();
        IExcelSource CreateExcelSource();

        ITextDestination CreateTextDestination();

        IJsonSource<T> CreateJsonSource<T>();
    }
}
