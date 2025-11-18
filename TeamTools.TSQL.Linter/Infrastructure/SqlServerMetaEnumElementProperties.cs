using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class SqlServerMetaEnumElementProperties
    {
        public string Name { get; set; }

        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
