using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines
{
    [ExcludeFromCodeCoverage]
    public sealed class SqlServerMetadata
    {
        public ICollection<string> Keywords { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> GlobalVariables { get; } = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, ICollection<EnumElementProperties>> Enums { get; } = new SortedDictionary<string, ICollection<EnumElementProperties>>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, SqlMetaProgrammabilitySignature> Functions { get; } = new SortedDictionary<string, SqlMetaProgrammabilitySignature>(StringComparer.OrdinalIgnoreCase);

        public class EnumElementProperties
        {
            public string Name { get; set; }

            public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
