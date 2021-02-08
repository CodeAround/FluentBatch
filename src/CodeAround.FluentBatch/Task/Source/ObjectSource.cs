using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CodeAround.FluentBatch.Interface;
using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Task.Generic;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Task;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class ObjectSource : WorkTask, IObjectSource
    {
        private IEnumerable DataSource { get; set; }
        private IDictionary<string, PropertyInfo> _cachedProperties;

        public ObjectSource(ILogger logger, bool useTrace)
           : base(logger, useTrace)
        {
        }

        public IObjectSource From<T>(IEnumerable<T> dataSource)
        {
            if (dataSource == null)
            {
                Trace("data source is null");
                throw new ArgumentNullException("data source");
            }

            this.DataSource = dataSource;
            _cachedProperties = GetProperties(typeof(T));
            Trace("Set From Data Source", dataSource);
            return this;
        }

        public IObjectSource From(IEnumerable dataSource)
        {
            if (dataSource == null)
            {
                Trace("data source is null");
                throw new ArgumentNullException("data source");
            }

            this.DataSource = dataSource;
            _cachedProperties = GetProperties(this.DataSource.GetType());
            Trace("Set From Data Source", dataSource);
            return this;
        }

        private IDictionary<string, PropertyInfo> GetProperties(Type t)
        {
            Trace("Get Propertis", t);
            IDictionary<string, PropertyInfo> dict = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] props = t.GetProperties();
            if (props == null)
                return null;
            foreach (PropertyInfo p in props)
            {
                if (p.CanRead)
                {
                    ParameterInfo[] indexParams = p.GetIndexParameters();
                    if (indexParams == null || indexParams.Length == 0)
                        dict[p.Name] = p;
                    Trace("Property", p);
                }
            }
            return dict;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;

            try
            {
                Trace("Start Object Source Execute");
                List<IRow> lst = new List<IRow>();

                if (this.DataSource == null)
                {
                    Trace("No data source set.");
                    throw new InvalidOperationException("No data source set.");
                }

                foreach (object obj in this.DataSource)
                {
                    var props = _cachedProperties;
                    if (props == null)
                        props = GetProperties(obj.GetType());
                    IRow r = new ObjectRow(obj, props);
                    lst.Add(r);
                }
                Trace("End Object Source Execute");

                result = new TaskResult(true, lst);
            }
            catch (Exception ex)
            {
                Log($"Error task : {ex.ToExceptionString()}", ex);
                Fault(ex);
                result = new TaskResult(false, null);
            }

            return result;
        }
    }
}