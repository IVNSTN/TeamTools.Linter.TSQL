using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0297", "FIXED_MERGE_ALIASES")]
    internal sealed class FixedMergeParticipantsAliasesRule : AbstractRule
    {
        private const string ExpectedTargetAlias = "trg";
        private const string ExpectedSourceAlias = "src";

        public FixedMergeParticipantsAliasesRule() : base()
        {
        }

        public override void Visit(MergeSpecification node)
        {
            ValidateAlias(node.Target, node.TableAlias, ExpectedTargetAlias);
            ValidateAlias(node.TableReference, ExpectedSourceAlias);
        }

        private void ValidateAlias(TableReference tbl, string expectedAlias)
        {
            if (tbl is TableReferenceWithAlias al)
            {
                ValidateAlias(tbl, al.Alias, expectedAlias);

                return;
            }

            HandleNodeError(tbl, expectedAlias);
        }

        private void ValidateAlias(TableReference tbl, Identifier alias, string expectedAlias)
        {
            if (alias != null && alias.Value.Equals(expectedAlias, StringComparison.Ordinal))
            {
                return;
            }

            // e.g. cte with correct name which is used for reference itself
            if (tbl is NamedTableReference tblName
                && tblName.SchemaObject.SchemaIdentifier is null
                && tblName.SchemaObject.BaseIdentifier.Value.Equals(expectedAlias, StringComparison.Ordinal))
            {
                return;
            }

            HandleNodeError(alias as TSqlFragment ?? tbl, expectedAlias);
        }
    }
}
