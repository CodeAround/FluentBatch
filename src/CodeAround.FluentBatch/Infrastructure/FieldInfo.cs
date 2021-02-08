using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Infrastructure
{
    public class FieldInfo
    {
        public FieldInfo(string sourceField, string destinationField, bool isSourceKey)
        {
            SourceField = sourceField;
            DestinationField = destinationField;
            IsSourceKey = isSourceKey;
        }

        public string SourceField { get; set; }

        public string DestinationField { get; set; }

        public bool IsSourceKey { get; set; }

    }
}
