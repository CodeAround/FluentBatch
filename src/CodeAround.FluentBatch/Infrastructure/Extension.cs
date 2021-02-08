using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Task.Generic;
using System.ComponentModel;

namespace CodeAround.FluentBatch.Infrastructure
{
    public static class Extension
    {
        public static DynamicParameters ToSqlParameters(this Dictionary<string, object> dict)
        {
            DynamicParameters parameters = null;

            if (dict.Any())
            {
                parameters = new DynamicParameters();

                foreach (var item in dict)
                {
                    parameters.Add($"@{item.Key}", item.Value);
                }
            }


            return parameters;
        }

        public static string ToInfo(this Dictionary<string, object> dic)
        {
            if (dic == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            dic.ToList().ForEach(x => sb.AppendFormat("Name: {0}, Value: {1}{2}", x.Key, x.Value, Environment.NewLine));

            return sb.ToString();
        }

        public static List<string> GetPrimaryKeys(this IDbConnection conn, string tableSchema, string tableName, IDbTransaction transaction = null)
        {
            string sql = @"select kcu.TABLE_SCHEMA, kcu.TABLE_NAME, kcu.CONSTRAINT_NAME, tc.CONSTRAINT_TYPE, kcu.COLUMN_NAME, kcu.ORDINAL_POSITION
                                        from INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc
                                        join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as kcu
                                        on kcu.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
                                        and kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                                        and kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
                                        and kcu.TABLE_NAME = tc.TABLE_NAME
                                        and kcu.TABLE_SCHEMA = '" + DbUtil.EscapeString(tableSchema) + @"'
                                        and kcu.TABLE_NAME = '" + DbUtil.EscapeString(tableName) + @"' 
                                        where tc.CONSTRAINT_TYPE in ( 'PRIMARY KEY' )";

            List<string> pkColumns = conn.Query(sql, transaction: transaction).Select(x => x.COLUMN_NAME as string).ToList();

            return pkColumns;
        }

        public static string BuildKeyWhereCondition(this IList<string> keys, IRow row, Dictionary<string, FieldInfo> mappedFields, out Dictionary<string, object> sqlParameters)
        {
            StringBuilder sb = new StringBuilder();
            sqlParameters = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                var mapped = mappedFields.Values.FirstOrDefault(x => x.DestinationField == key);
                if (mapped != null)
                {
                    if (sb.Length > 0)
                        sb.Append(" AND ");

                    sb.AppendFormat("[{0}] = @{0}", key);
                    sqlParameters.Add($"@{key}", row[mapped.SourceField]);
                }
            }

            return sb.ToString();
        }

        public static string ToInsertStatement(this IDbConnection conn, string tableName, string schema, IRow row, Dictionary<string, FieldInfo> mappedFields, Dictionary<string, object> otherFiels, IDbTransaction transaction, out DynamicParameters sqlParameters)
        {
            bool isMapped;
            StringBuilder sb = new StringBuilder();
            StringBuilder ssb = new StringBuilder();
            StringBuilder tssb = new StringBuilder();
            sqlParameters = new DynamicParameters();

            string statement = $@"select Column_Name as ColumnName from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = '{DbUtil.EscapeString(schema) }' and TABLE_NAME = '{DbUtil.EscapeString(tableName) }'";
            string tableNameWithSchema = $"{schema}.{tableName}";
            var res = conn.Query(statement, transaction: transaction);

            if (res != null && res.Count() > 0)
            {
                sb.Append($"INSERT INTO {DbUtil.EscapeString(tableNameWithSchema)} (");
                foreach (var col in res)
                {
                    isMapped = true;
                    object sourceValue = null;
                    var map = mappedFields.Values.FirstOrDefault(x => x.DestinationField == col.ColumnName);

                    if (map == null)
                    {
                        if (otherFiels.ContainsKey(col.ColumnName))
                            sourceValue = otherFiels[col.ColumnName];
                        else
                            isMapped = false;
                    }
                    else
                        sourceValue = row[map.SourceField];

                    if (isMapped)
                    {
                        if (sourceValue == DBNull.Value)
                            sourceValue = null;

                        if (ssb.Length > 0)
                            ssb.Append(",");
                        ssb.Append(col.ColumnName);

                        if (tssb.Length > 0)
                            tssb.Append(",");
                        tssb.Append($"@{col.ColumnName}");

                        sqlParameters.Add($"@{col.ColumnName}", sourceValue);
                    }
                }

                sb.Append(ssb.ToString());
                sb.Append(") VALUES (");
                sb.Append(tssb.ToString());
                sb.Append(") ");
            }

            return sb.ToString();
        }

        public static string ToUpdateStatement(this IDbConnection conn, string tableName, string schema, IRow row, IList<string> keys, Dictionary<string, FieldInfo> mappedFields, Dictionary<string, object> otherFiels, IDbTransaction transaction, bool updateAllFields, out DynamicParameters sqlParameters)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder ssb = new StringBuilder();

            sqlParameters = new DynamicParameters();
            var keySqlParameters = new Dictionary<string, object>();

            string statement = $@"select Column_Name as ColumnName from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = '{DbUtil.EscapeString(schema)}' and TABLE_NAME = '{DbUtil.EscapeString(tableName) }'";
            var keyCondition = keys.BuildKeyWhereCondition(row, mappedFields, out keySqlParameters);

            var res = conn.Query(statement, transaction: transaction);

            if (res != null && res.Count() > 0)
            {
                sb.Append($"UPDATE {schema}.{tableName} SET ");
                foreach (var col in res)
                {
                    object sourceValue = null;
                    var map = mappedFields.Values.FirstOrDefault(x => x.DestinationField == col.ColumnName);

                    if (map == null)
                    {
                        if (updateAllFields)
                        {
                            if (row.Fields.Contains(col.ColumnName as string))
                                sourceValue = row[col.ColumnName];
                            else if (otherFiels.ContainsKey(col.ColumnName))
                                sourceValue = otherFiels[col.ColumnName];
                        }
                    }
                    else
                        sourceValue = row[map.SourceField];

                    if (sourceValue == DBNull.Value)
                        sourceValue = null;

                    if (!keys.Contains(col.ColumnName as string))
                    {
                        if (updateAllFields || map != null)
                        {
                            if (ssb.Length > 0)
                                ssb.Append(",");
                            ssb.Append($"{col.ColumnName} = @{col.ColumnName}");

                            sqlParameters.Add($"@{col.ColumnName}", sourceValue);
                        }
                    }
                }

                sb.Append(ssb.ToString());

                if (!String.IsNullOrEmpty(keyCondition) && keySqlParameters != null)
                {
                    sb.Append(" WHERE ");
                    sb.Append(keyCondition);

                    foreach (var dic in keySqlParameters)
                    {
                        if (!sqlParameters.ParameterNames.Contains(dic.Key))
                            sqlParameters.Add(dic.Key, dic.Value);
                    }
                }
            }

            return sb.ToString();
        }

        public static string ToDeleteStatement(this IDbConnection conn, string tableName, string schema, IRow row, IList<string> keys, Dictionary<string, FieldInfo> mappedFields, IDbTransaction transaction, out DynamicParameters sqlParameters)
        {
            StringBuilder sb = new StringBuilder();

            sqlParameters = new DynamicParameters();
            var keySqlParameters = new Dictionary<string, object>();

            var keyCondition = keys.BuildKeyWhereCondition(row, mappedFields, out keySqlParameters);

            sb.Append($"DELETE FROM {schema}.{tableName} ");

            if (!String.IsNullOrEmpty(keyCondition) && keySqlParameters != null)
            {
                sb.Append("WHERE ");
                sb.Append(keyCondition);

                foreach (var dic in keySqlParameters)
                {
                    if (!sqlParameters.ParameterNames.Contains(dic.Key))
                        sqlParameters.Add(dic.Key, dic.Value == DBNull.Value ? null : dic.Value);
                }
            }

            return sb.ToString();
        }

        public static Dictionary<String, Object> DynamicToDictionary(dynamic dynObj)
        {
            var dictionary = new Dictionary<string, object>();
            var props = TypeDescriptor.GetProperties(dynObj);
            foreach (PropertyDescriptor propertyDescriptor in props)
            {
                object obj = propertyDescriptor.GetValue(dynObj);

                if (!dictionary.ContainsKey(propertyDescriptor.Name))
                    dictionary.Add(propertyDescriptor.Name, obj);
            }
            return dictionary;
        }
    }
}
