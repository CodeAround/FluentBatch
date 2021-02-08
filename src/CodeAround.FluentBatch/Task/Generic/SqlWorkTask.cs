using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeAround.FluentBatch.Interface.Task;
using Dapper;
using System.Data;
using CodeAround.FluentBatch.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Generic
{
    public class SqlWorkTask : WorkTask, ISqlWorkTask
    {
        private string _statement;
        private string _filename;
        private string _resourceFilename;
        private Assembly _assembly;
        private IDbConnection _connection;
        private StatementCommandType _statementType;
        private Dictionary<string, object> _taskParameters;
        private bool _usingLoopValue;
        private string _parameterName;

        public SqlWorkTask(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {
            _taskParameters = new Dictionary<string, object>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize SqlWorkTask", taskResult);
            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                if (_usingLoopValue)
                {
                    var loopValue = ((LoopTaskResult)TaskResult).LoopValue;
                    if (!String.IsNullOrEmpty(_parameterName) && loopValue != null && !_taskParameters.ContainsKey(_parameterName))
                        _taskParameters.Add(_parameterName, loopValue);
                }

            }
        }

        public ISqlWorkTask FromStatement(string statement)
        {
            if (String.IsNullOrEmpty(statement))
            {
                Trace(String.Format("statement is null"));
                throw new ArgumentNullException("statement is null");
            }

            Trace(String.Format("Create sql work task type. Statement : {0}", statement));

            _statement = statement;
            Trace("Set FromStatement", statement);
            return this;
        }

        public ISqlWorkTask FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                Trace("statement is null");
                throw new ArgumentNullException("filename is null");
            }

            Trace(String.Format("Create sql work task type. Filename : {0}", filename));

            _filename = filename;
            Trace("Set FromFile", filename);
            return this;
        }

        public ISqlWorkTask FromResource(string resourceFilename, Assembly assembly)
        {
            if (String.IsNullOrEmpty(resourceFilename))
            {
                Trace(String.Format("resource filename is null"));
                throw new ArgumentNullException("resource filename is null");
            }

            if (assembly == null)
            {
                Trace(String.Format("assembly is null"));
                throw new ArgumentNullException("assembly is null");
            }

            Trace("Set Resource", resourceFilename);

            _assembly = assembly;
            _resourceFilename = resourceFilename;
            return this;
        }

        public ISqlWorkTask WithStatementType(StatementCommandType statementType)
        {
            Trace("Set Statement Type", statementType);

            _statementType = statementType;
            return this;
        }

        public ISqlWorkTask Connection(IDbConnection conn)
        {
            if (conn == null)
            {
                Trace("connection is null");
                throw new ArgumentNullException("connection is null");
            }

            this._connection = conn;
            Trace("Set Connection", conn);
            return this;
        }

        public ISqlWorkTask AddParameter(string name, object value)
        {
            if (!String.IsNullOrEmpty(name) && value != null && !_taskParameters.ContainsKey(name))
                _taskParameters.Add(name, value);

            Trace("Set Add parameter", new { Name = name, Value = value });

            return this;
        }

        public ISqlWorkTask UseLoopValue(bool usingLoopValue, string parameterName)
        {
            if (usingLoopValue)
            {
                _usingLoopValue = usingLoopValue;
                _parameterName = parameterName;
            }
            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;
            object queryResult = null;
            try
            {
                Trace("Start Sql Work Task Execute");
                string statement = string.Empty;

                if (_connection == null)
                {
                    Trace("Connection is null");
                    throw new ArgumentNullException("Connection is null");
                }

                if (!String.IsNullOrEmpty(_statement))
                {
                    Trace("Statement : {0}", statement);
                    statement = _statement;
                }
                else if (!String.IsNullOrEmpty(_resourceFilename) && _assembly != null)
                {
                    Trace("ResourceFilename : {0}", _resourceFilename);
                    statement = ReadResource(_resourceFilename, _assembly);
                }
                else if (!String.IsNullOrEmpty(_filename))
                {
                    Trace("File Name : {0}", _filename);
                    statement = ReadFile(_filename);
                }

                if (!String.IsNullOrEmpty(statement))
                {
                    Trace("Statement Type : {0}", _statementType);
                    if (_statementType == StatementCommandType.Query)
                    {
                        if (_taskParameters != null && _taskParameters.Count > 0)
                            queryResult = _connection.Query(statement, _taskParameters.ToSqlParameters()).ToList();
                        else
                            queryResult = _connection.Query(statement).ToList();

                        Trace(String.Format("SqlWorkTask Query Statements: {0}, parameters: {1}", _statement,
                            _taskParameters.ToInfo()));
                    }

                    if (_statementType == StatementCommandType.Command)
                    {
                        if (_taskParameters != null && _taskParameters.Count > 0)
                            queryResult = _connection.Execute(statement, _taskParameters.ToSqlParameters());
                        else
                            queryResult = _connection.Execute(statement);

                        Trace(String.Format(
                            "SqlWorkTask Command Statements: {0}, affected rows: {1}, parameters: {2}", _statement, result,
                            _taskParameters.ToInfo()));
                    }
                }
                result = new TaskResult(true, queryResult);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }
            return result;
        }

        private string ReadResource(string filename, Assembly assembly)
        {
            var resourceName = assembly.GetName().Name + "." + filename;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        private string ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Trace("file name is null");
                throw new ArgumentNullException("file name is null");
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        public override void Finish()
        {
            base.Finish();
            _taskParameters.Clear();
        }
    }
}
