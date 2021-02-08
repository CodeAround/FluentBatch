using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dapper;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Task.Base;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Task;
using CodeAround.FluentBatch.Interface.Base;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class SqlSource : SqlBase, ISqlSource
    {
        public SqlSource(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            this.CommandParameters = new Dictionary<string, object>();
            this.CommandType = CommandType.Text;
        }

        public ISqlSource FromTable(string tableName, string schema)
        {
            if (String.IsNullOrEmpty(tableName))
            {
                Trace("table name is null");
                throw new ArgumentNullException("table name is null");
            }

            this.CommandText = $"SELECT * FROM {CommandSchema}.{tableName}";
            this.CommandType = CommandType.Text;

            this.CommandSchema = schema;
            return this;
        }

        public ISqlSource FromQuery(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                Trace("query is null");
                throw new ArgumentNullException("query is null");
            }

            this.CommandText = query;
            this.CommandType = CommandType.Text;
            Trace("Set Query", query);
            return this;
        }

        public ISqlSource WithCommandTimeout(int timeout)
        {
            this.CommandTimeout = timeout;

            Trace("Set Timeout", timeout);
            return this;
        }

        public ISqlSource FromStoredProcedure(string storedProc)
        {
            if (String.IsNullOrEmpty(storedProc))
            {
                Trace("stored procedure is null");
                throw new ArgumentNullException("stored procedure is null");
            }

            this.CommandText = storedProc;
            this.CommandType = CommandType.StoredProcedure;
            Trace("Set Store Procedure", storedProc);
            return this;
        }

        public ISqlSource UseConnection(IDbConnection connection)
        {
            if (connection == null)
            {
                Trace("connection string is nul");
                throw new ArgumentNullException("connection string is null");
            }

            this.Connection = connection;
            Trace("Set connection", connection);
            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;

            try
            {
                if (Connection == null)
                {
                    Trace("connection is nul");
                    throw new ArgumentNullException("connection is null");
                }

                if (String.IsNullOrEmpty(CommandText))
                {
                    Trace("command text is null");
                    throw new ArgumentNullException("command text is null");
                }

                var rows = GetRows();
                result = new TaskResult(true, rows.ToList());
            }
            catch (Exception ex)
            {
                Trace($"Error task : {ex.ToExceptionString()}");
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }

        private IEnumerable<IRow> GetRows()
        {
            List<IRow> rows = new List<IRow>();

            var newConn = (IDbConnection)Activator.CreateInstance(Connection.GetType());
            newConn.ConnectionString = Connection.ConnectionString;
            using (var conn = newConn)
            {
                var result = conn.Query(CommandText, CommandParameters.ToSqlParameters(), null, true, commandTimeout: this.CommandTimeout, this.CommandType);

                if (result != null && result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        var dic = Extension.DynamicToDictionary(item);
                        Trace("GetRows - Dic", dic);
                        yield return new DictionaryRow(dic);
                    }
                }
            }
        }

        public ISqlSource AddParameter(string name, object value)
        {
            if (!String.IsNullOrEmpty(name) && value != null && !this.CommandParameters.ContainsKey(name))
                this.CommandParameters.Add(name, value);

            Trace("command parameter", new { Name = name, Value = value });
            return this;
        }

        public override void Finish()
        {
            base.Finish();
            this.CommandParameters.Clear();
        }
    }
}

