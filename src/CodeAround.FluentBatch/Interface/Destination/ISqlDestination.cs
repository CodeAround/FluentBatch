using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Builder;

namespace CodeAround.FluentBatch.Interface.Destination
{
    public interface ISqlDestination : IFault
    {
        ISqlDestination Map(Func<string> sourceField, Func<string> destinationField, bool isSourceKey);
        ISqlDestination Specify(Func<string> field, Func<object> value);
        ISqlDestination UseConnection(IDbConnection connection);
        ISqlDestination IdentityInsert();
        ISqlDestination DeleteFirst();
        ISqlDestination TruncteFirst();
        ISqlDestination Table(string tableName);
        ISqlDestination Schema(string schema);
        ISqlDestination UseTransaction();
        ISqlDestination UpdateAllFields();
        ISqlDestination WithCommandTimeout(int timeout);
    }
}
