using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Routines
{
    public sealed class LintingConfig : BaseLintingConfig
    {
        public LintingConfig()
        {
            // TODO : extract to config
            SupportedFiles.Add("SQL", new List<string> { ".sql", ".tsql" });
        }

        public IDictionary<string, string> Deprecations { get; } = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> SpecialCommentPrefixes { get; } = new List<string>();

        public int CompatibilityLevel { get; set; } = 150;

        public SqlServerMetadata SqlServerMetadata { get; } = new SqlServerMetadata();
    }
}
