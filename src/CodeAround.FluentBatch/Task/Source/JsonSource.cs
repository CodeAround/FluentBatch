using CodeAround.FluentBatch.Infrastructure;
using CodeAround.FluentBatch.Interface.Base;
using CodeAround.FluentBatch.Interface.Source;
using CodeAround.FluentBatch.Task.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CodeAround.FluentBatch.Task.Source
{
    public class JsonSource<T>: WorkTask, IJsonSource<T>
    {
        private string _inputFile;
        private LoopSource _loopSource;
        private string _inputJsonSource;
        private string _name;
        private Dictionary<string, string> _mappedFields;
        private string _collectionPropertyName;

        public JsonSource(ILogger logger, bool useTrace) 
            : base(logger, useTrace)
        {
            _mappedFields = new Dictionary<string, string>();
        }

        public override void Initialize(TaskResult taskResult)
        {
            Trace("Start Initialize JsonSource", taskResult);
            base.Initialize(taskResult);
            if (TaskResult is LoopTaskResult)
            {
                var input = ((LoopTaskResult)TaskResult).LoopValue.ToString();
                switch (_loopSource)
                {
                    case LoopSource.Filename:
                        _inputFile = input;
                        break;
                    case LoopSource.String:
                        _inputJsonSource = input;
                        break;
                }
            }
        }

        public IJsonSource<T> FromFile(string file)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException("File is null");

            this._inputFile = file;
            Trace(String.Format("Set File: {0} : ", file));
            return this;
        }

        public IJsonSource<T> LoopBehaviour(LoopSource loopSource)
        {
            this._loopSource = loopSource;
            Trace("Set LoopBehavior", loopSource);
            return this;
        }

        public IJsonSource<T> FromString(string jsonSource)
        {
            if (String.IsNullOrEmpty(jsonSource))
                throw new ArgumentNullException("String json is null");

            this._inputJsonSource = jsonSource;
            Trace("Set JsonSource", jsonSource);
            return this;
        }

        public IJsonSource<T> CollectionPropertyName(string collectionPropertyName)
        {
            if (String.IsNullOrEmpty(collectionPropertyName))
            {
                Trace("Collection property name is null");
                throw new ArgumentNullException("Collection property name is null");
            }

            this._collectionPropertyName = collectionPropertyName;
            
            return this;
        }

        public IJsonSource<T> Map(Func<string> sourceField, Func<string> destinationField)
        {
            Trace(String.Format("Set Map: sourceField {0} - destinationField {1} ", sourceField(), destinationField()));
            string source = sourceField();
            if (!_mappedFields.ContainsKey(source))
                _mappedFields.Add(source, destinationField());

            return this;
        }

        public override TaskResult Execute()
        {
            TaskResult result = null;

            try
            {
                List<IRow> lst = new List<IRow>();

                Trace("Process json string");

                if (this._inputFile != null)
                {
                    Trace(String.Format("Input File : {0} ", this._inputFile));
                    using (StreamReader str = new StreamReader(this._inputFile))
                    {
                        lst = LoadRow(str.ReadToEnd());
                    }

                }
                else if (this._inputJsonSource != null)
                {
                    Trace(String.Format("Json Source: {0} ", this._inputJsonSource));

                    lst = LoadRow(_inputJsonSource);
                }
                else
                    throw new InvalidOperationException("No file/stream set.");

                Trace("End process");
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

        private List<IRow> LoadRow(string inputJson)
        {
            List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();

            Dictionary<string, object> dicRow = null;

            var obj = (T)JsonConvert.DeserializeObject<T>(inputJson);

            var collProp = obj.GetType().GetProperty(_collectionPropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.IgnoreCase);
            
            if(collProp == null)
            {
                Trace("Collection property is null");
                throw new ArgumentNullException("Collection property is null");
            }

            var coll = (IEnumerable)collProp.GetValue(obj);

            if (coll == null)
            {
                Trace("Collection is null");
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in coll)
            {
                dicRow = new Dictionary<string, object>();
                Trace("Mapped Fields", _mappedFields);
                foreach (var field in _mappedFields)
                {
                    var itemProp = item.GetType().GetProperty(field.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.IgnoreCase);

                    if(itemProp == null)
                    {
                        Trace($"item {field.Key} is null in collection");
                        throw new ArgumentNullException($"item {field.Key} is null in collection");
                    }

                    var value = itemProp.GetValue(item);

                    if(value == null)
                    {
                        Trace($"the value of the item {field.Key} is null in collection");
                        throw new ArgumentNullException($"the value of the item {field.Key} is null in collection");
                    }

                    dicRow.Add(field.Value, value);
                }

                if (dicRow.Count > 0)
                    values.Add(dicRow);
                
            }

            Trace("Dictionary Values", values);

            return values.Select(x => new DictionaryRow(x)).Cast<IRow>().ToList();
        }
    }
}
