using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Interface.Base;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Base
{
    public abstract class SqlBase : WorkTask
    {
        public SqlBase(ILogger logger, bool useTrace)
            : base(logger, useTrace)
        {

        }
        protected Dictionary<string, object> CommandParameters;
        protected IDbConnection Connection { get; set; }
        protected CommandType CommandType { get; set; }
        protected string CommandText { get; set; }
        protected string CommandSchema { get; set; }
        protected int? CommandTimeout { get; set; }

    }
}
