using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public static class ExceptionExtension
    {
        public static string ToExceptionString(this System.Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(ex.Message))
            {
                sb.AppendFormat("Message = '{0}'", ex.Message.Trim());
            }

            if (!String.IsNullOrEmpty(ex.Source))
            {
                if (sb.Length > 0)
                    sb.Append(" | ");

                sb.AppendFormat("Source = '{0}'", ex.Source.Trim());
            }

            if (!String.IsNullOrEmpty(ex.StackTrace))
            {
                if (sb.Length > 0)
                    sb.Append(" | ");

                sb.AppendFormat("StackTrace = '{0}'", ex.StackTrace.Trim());
            }

            if (ex.InnerException != null)
            {
                if (sb.Length > 0)
                    sb.Append(" | ");

                sb.AppendFormat("Inner Exception = '{0}'", ex.InnerException.ToExceptionString());
            }

            return sb.ToString();
        }

        public static string ToExceptionString(this System.Exception ex, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();
            string result = ToExceptionString(ex);

            if (fields != null && fields.Count > 0)
            {
                foreach (var item in fields)
                {
                    if (sb.Length > 0)
                        sb.Append(" | ");

                    sb.AppendFormat("{0}: {1}", item.Key, item.Value);
                }
            }

            if (sb.Length > 0)
                result = String.Format("{0}. Other Fields", result, sb.ToString());

            return result;
        }
    }
}
