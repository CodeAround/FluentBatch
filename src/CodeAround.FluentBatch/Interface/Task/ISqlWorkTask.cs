using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Builder;
using CodeAround.FluentBatch.Interface.Source;

namespace CodeAround.FluentBatch.Interface.Task
{
    public interface ISqlWorkTask : IFault
    {
        ISqlWorkTask FromStatement(string statement);

        ISqlWorkTask WithStatementType(StatementCommandType statementType);

        ISqlWorkTask FromFile(string filename);

        ISqlWorkTask FromResource(string resourceFilename, Assembly assembly);

        ISqlWorkTask Connection(IDbConnection conn);

        ISqlWorkTask AddParameter(string name, object value);
        ISqlWorkTask UseLoopValue(bool usingLoopValue, string parameterName);
    }
}
