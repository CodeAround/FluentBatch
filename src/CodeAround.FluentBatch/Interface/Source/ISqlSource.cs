using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Source
{
    public interface ISqlSource : IFault
    {
        ISqlSource FromTable(string tableName, string schema);
        ISqlSource FromQuery(string query);
        ISqlSource FromStoredProcedure(string storedProc);
        ISqlSource UseConnection(IDbConnection connection);
        ISqlSource AddParameter(string name, object value);
        ISqlSource WithCommandTimeout(int timeout);
    }
}
