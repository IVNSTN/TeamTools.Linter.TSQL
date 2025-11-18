using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class SqlServerMetaTypeDescription
    {
        public string Name { get; set; }

        public bool IsAlias => AlsoKnownAs != null;

        public string AlsoKnownAs { get; set; } = null;

        public bool ForceToOriginalName { get; set; } = false;

        public int Precedence { get; set; }

        public bool CanBeNativelyCompiled { get; set; } = true;
    }
}
