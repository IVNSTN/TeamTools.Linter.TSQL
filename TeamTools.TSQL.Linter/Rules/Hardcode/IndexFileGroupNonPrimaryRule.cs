using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0915", "NON_PRIMARY_FILEGROUP")]
    [IndexRule]
    internal sealed class IndexFileGroupNonPrimaryRule : AbstractRule
    {
        public IndexFileGroupNonPrimaryRule() : base()
        {
        }

        public override void Visit(FileGroupOrPartitionScheme node)
        {
            // no filegroup or partitionsceme given - both cases are good
            if (node.Name?.Value is null || node.PartitionSchemeColumns?.Count > 0)
            {
                return;
            }

            if (node.Name.Value.Equals(TSqlDomainAttributes.DefaultFileGroup, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
