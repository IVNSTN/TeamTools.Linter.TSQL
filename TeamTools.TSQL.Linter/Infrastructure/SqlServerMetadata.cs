using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Infrastructure;

namespace TeamTools.TSQL.Linter.Routines
{
    [ExcludeFromCodeCoverage]
    public sealed class SqlServerMetadata
    {
        public ICollection<string> Keywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> GlobalVariables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, List<SqlServerMetaEnumElementProperties>> Enums { get; } = new Dictionary<string, List<SqlServerMetaEnumElementProperties>>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, SqlServerMetaProgrammabilitySignature> Functions { get; } = new Dictionary<string, SqlServerMetaProgrammabilitySignature>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, SqlServerMetaTypeDescription> Types { get; } = new Dictionary<string, SqlServerMetaTypeDescription>(StringComparer.OrdinalIgnoreCase);
    }
}
