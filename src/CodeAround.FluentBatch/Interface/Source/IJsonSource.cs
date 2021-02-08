using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface IJsonSource<T> : IFault
    {
        IJsonSource<T> FromFile(string file);

        IJsonSource<T> LoopBehaviour(LoopSource loopSource);

        IJsonSource<T> FromString(string jsonSource);

        IJsonSource<T> CollectionPropertyName(string collectionPropertyName);

        IJsonSource<T> Map(Func<string> sourceField, Func<string> destinationField);

    }
}
