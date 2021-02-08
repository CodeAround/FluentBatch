using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Task.Base;
using CodeAround.FluentBatch.Engine;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Destination;
using CodeAround.FluentBatch.Interface.Task;
using Dapper;
using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Destination
{
    public class SqlDestination : SqlBase, ISqlDestination
    {
        private Dictionary<string, FieldInfo> _mappedFields;
        private Dictionary<string, object> _otherFields;
        private IEnumerable<IRow> _arr = null;
        private bool _identInsert;
        private bool _deleteFirst;
        private bool _truncateFirst;
        private string _tableName;
        private string _schema;
        private bool _useTransaction;
        private bool _updateAllFields;

        public SqlDestination(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            _mappedFields = new Dictionary<string, FieldInfo>();
            _otherFields = new Dictionary<string, object>();
            _useTransaction = false;
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize SqlDestination", taskResult);

            base.Initialize(taskResult);

            if (TaskResult is LoopTaskResult)
            {
                _arr = (IEnumerable<IRow>)((LoopTaskResult)TaskResult).Result;
            }
            else
            {
                _arr = (IEnumerable<IRow>)TaskResult.Result;
            }
        }

        public override TaskResult Execute()
        {
            IDbTransaction transaction = null;
            TaskResult result = new TaskResult(true, null);
            try
            {
                Trace("Start Execute", null);
                Dictionary<string, object> whereSqlParameters = null;
                if (_arr != null && _arr.Count() > 0)
                {
                    var newConn = (IDbConnection)Activator.CreateInstance(Connection.GetType());
                    newConn.ConnectionString = Connection.ConnectionString;
                    using (var conn = newConn)
                    {
                        try
                        {
                            conn.Open();
                            Trace("Use Transaction", _useTransaction);
                            if (_useTransaction)
                            {
                                transaction = conn.BeginTransaction();
                            }

                            Trace("Connection String", conn.ConnectionString);
                            if (_truncateFirst)
                            {
                                Trace("Execute truncate", _truncateFirst);
                                string stmt = $"TRUNCATE TABLE {_schema}.{_tableName}";
                                conn.Execute(stmt);
                            }

                            foreach (var row in _arr)
                            {
                                Trace("Current Row", row.Operation);
                                if (row.Operation != OperationType.None)
                                {
                                    var lst = conn.GetPrimaryKeys(_schema, _tableName, transaction: transaction);

                                    if (_deleteFirst)
                                    {
                                        Trace("Execute Delete", _deleteFirst);
                                        var keyWhereCondition = lst.BuildKeyWhereCondition(row, _mappedFields, out whereSqlParameters);
                                        Trace("Use KeyWhereCondition", keyWhereCondition);
                                        Trace("Use whereSqlParameters", whereSqlParameters);
                                        string stmt = $"DELETE FROM {_schema}.{_tableName}";

                                        if (!String.IsNullOrEmpty(keyWhereCondition))
                                            stmt = stmt + " WHERE " + keyWhereCondition;

                                        conn.Execute(stmt, whereSqlParameters.ToSqlParameters(), transaction: transaction);
                                    }

                                    Process(conn, row, lst, transaction: transaction);
                                }
                            }

                            if (_useTransaction && transaction != null)
                            {
                                transaction.Commit();
                                Trace($"Commit Transaction", null);
                            }

                            Trace($"End Execute", null);
                            result = new TaskResult(true, _arr);
                        }
                        catch (Exception ex)
                        {
                            if (_useTransaction && transaction != null)
                            {
                                transaction.Rollback();
                                Log($"Error task while use transaction", ex);
                            }

                            Log($"Error task : {ex.ToExceptionString()}", ex);
                            Fault(ex);
                            result = new TaskResult(false, null);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }
            return result;
        }

        private void Process(IDbConnection conn, IRow row, List<string> keys, IDbTransaction transaction = null)
        {
            Trace("Start Process: Connection Obj", conn.ConnectionString);
            StringBuilder statement = new StringBuilder();
            DynamicParameters sqlParameters = null;
            Trace("Row Operation. Row", row);

            if (row != null)
            {
                if (_identInsert)
                {
                    Trace("SET IDENTITY_INSERT ON Table", this._tableName);
                    statement.Append($"SET IDENTITY_INSERT {_schema}.{_tableName} ON ");
                }

                switch (row.Operation)
                {
                    case OperationType.Insert:
                        statement.Append(conn.ToInsertStatement(_tableName, _schema, row, _mappedFields, _otherFields, transaction: transaction, out sqlParameters));
                        break;
                    case OperationType.Update:
                        statement.Append(conn.ToUpdateStatement(_tableName, _schema, row, keys, _mappedFields, _otherFields, transaction: transaction, _updateAllFields, out sqlParameters));
                        break;
                    case OperationType.Delete:
                        statement.Append(conn.ToDeleteStatement(_tableName, _schema, row, keys, _mappedFields, transaction: transaction, out sqlParameters));
                        break;
                }

                if (_identInsert)
                {
                    Trace("SET IDENTITY_INSERT OFF table", this._tableName);
                    statement.Append($" SET IDENTITY_INSERT {_schema}.{_tableName} OFF");
                }

                Trace("Process statement", statement);
                Trace("Process statement parameters", sqlParameters);
                if (!String.IsNullOrEmpty(statement.ToString()) && sqlParameters != null)
                {
                    conn.Execute(statement.ToString(), sqlParameters, transaction: transaction, commandTimeout: CommandTimeout);
                }
            }
        }

        public ISqlDestination Map(Func<string> sourceField, Func<string> destinationField, bool isSourceKey)
        {
            Trace(String.Format("Set Map: sourceField {0} - destinationField {1} - isSourceKey {2}", sourceField(), destinationField(), isSourceKey), null);
            string source = sourceField();
            if (!_mappedFields.ContainsKey(source))
                _mappedFields.Add(source, new FieldInfo(source, destinationField(), isSourceKey));

            return this;
        }

        public ISqlDestination WithCommandTimeout(int timeout)
        {
            this.CommandTimeout = timeout;

            Trace("Set Timeout", timeout);
            return this;
        }

        public ISqlDestination Specify(Func<string> field, Func<object> value)
        {
            Trace(String.Format("Set Specify: field {0} - value {1}", field(), value()), null);
            string source = field();
            if (!_otherFields.ContainsKey(source))
                _otherFields.Add(source, value());

            return this;
        }

        public ISqlDestination IdentityInsert()
        {
            Trace("Set IdentityInsert", true);
            _identInsert = true;
            return this;
        }

        public ISqlDestination DeleteFirst()
        {
            Trace("Set DeleteFirst", true);
            _deleteFirst = true;
            return this;
        }

        public ISqlDestination UpdateAllFields()
        {
            Trace("Set UpdateAllFields", true);
            _updateAllFields = true;
            return this;
        }

        public ISqlDestination TruncteFirst()
        {
            Trace("Set Truncate First", true);
            _truncateFirst = true;
            return this;
        }

        public ISqlDestination UseConnection(IDbConnection connection)
        {
            this.Connection = connection;
            Trace("Set Connection", Connection.ConnectionString);
            return this;
        }

        public ISqlDestination Table(string tableName)
        {
            _tableName = tableName;
            Trace("Set TableName", _tableName);
            return this;
        }

        public ISqlDestination Schema(string schema)
        {
            Trace("Set schema", schema);
            _schema = schema;
            return this;
        }
        public ISqlDestination UseTransaction()
        {
            Trace("Set UseTransaction", true);
            _useTransaction = true;
            return this;
        }
    }
}