
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class ObjectBase
    {
        public bool UseTrace { get; protected set; }
        public ILogger Logger { get; protected set; }

        public ObjectBase(ILogger logger, bool useTrace)
        {
            Logger = logger;
            UseTrace = useTrace;
        }

        public ObjectBase()
        {

        }

        public void Trace(string message, object obj = null)
        {
            if (UseTrace)
            {
                string serializedObj = string.Empty;
                if (obj != null)
                    serializedObj = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                Logger.Log(LogLevel.Trace, string.Format("{0} | Serialized Obj: {1}", message, serializedObj));
            }
        }

        public void Log(string message, Exception ex = null, object obj = null)
        {
            if (UseTrace)
            {
                string serializedObj = string.Empty;
                if (obj != null)
                    serializedObj = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                Logger.Log(LogLevel.Error, ex, string.Format("{0} | Serialized Obj: {1}", message, serializedObj));
            }
        }
    }
}
