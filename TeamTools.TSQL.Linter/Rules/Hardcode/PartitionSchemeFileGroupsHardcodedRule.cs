using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0916", "PARTITION_SCHEME_FILEGROUPS_HARDCODED")]
    internal sealed class PartitionSchemeFileGroupsHardcodedRule : AbstractRule
    {
        private static readonly string Suggestion = Strings.ViolationDetails_PartitionSchemeHardcoded_UseAllToPrimary + " ALL TO " + TSqlDomainAttributes.DefaultFileGroupQuoted;

        public PartitionSchemeFileGroupsHardcodedRule() : base()
        {
        }

        public override void Visit(CreatePartitionSchemeStatement node)
        {
            if (!node.IsAll)
            {
                HandleNodeError(node, Suggestion);
            }

            ValidateFileGroups(node.FileGroups);
        }

        private void ValidateFileGroups(IList<IdentifierOrValueExpression> fg)
        {
            if (fg.Count > 1)
            {
                HandleNodeError(fg[0], Suggestion);
            }

            var badFg = fg
                .FirstOrDefault(f => f.Value != null
                    && !f.Value.Equals(TSqlDomainAttributes.DefaultFileGroup, StringComparison.OrdinalIgnoreCase));

            HandleNodeErrorIfAny(badFg, Suggestion);
        }
    }
}
