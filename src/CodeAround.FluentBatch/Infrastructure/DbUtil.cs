using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeAround.FluentBatch.Interface.Base;

namespace CodeAround.FluentBatch.Infrastructure
{
    internal static class DbUtil
    {
        public static string EscapeString(string s)
        {
            return s.Replace("'", "''");
        }
    }
}
