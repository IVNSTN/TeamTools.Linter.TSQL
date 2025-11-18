using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class SqlServerMetaProgrammabilitySignature
    {
        public string DataType { get; set; }

        public int ParamCount { get; set; } = -1;

        public int ParamCountMin { get; set; } = -1;

        public int ParamCountMax { get; set; } = -1;

        // name/index, type
        public IDictionary<string, string> ParamDefinition { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
