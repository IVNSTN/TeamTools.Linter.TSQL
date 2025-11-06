using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0926", "REDUNDANT_NOT_FOR_REPLICATION")]
    internal sealed class RedundantNotForReplicationRule : AbstractRule
    {
        public RedundantNotForReplicationRule() : base()
        {
        }

        public override void Visit(ForeignKeyConstraintDefinition node) => DetectRedundantOption(node, node.NotForReplication);

        public override void Visit(CheckConstraintDefinition node) => DetectRedundantOption(node, node.NotForReplication);

        public override void Visit(TriggerStatementBody node) => DetectRedundantOption(node, node.IsNotForReplication);

        public override void Visit(IdentityOptions node) => DetectRedundantOption(node, node.IsIdentityNotForReplication);

        public override void Visit(AlterTableAlterColumnStatement node)
        {
            bool hasNotForReplOptions =
                ((node.AlterTableAlterColumnOption & AlterTableAlterColumnOption.DropNotForReplication) == AlterTableAlterColumnOption.DropNotForReplication)
                || ((node.AlterTableAlterColumnOption & AlterTableAlterColumnOption.AddNotForReplication) == AlterTableAlterColumnOption.AddNotForReplication);

            DetectRedundantOption(node, hasNotForReplOptions);
        }

        private void DetectRedundantOption(TSqlFragment node, bool notForReplication)
        {
            if (notForReplication)
            {
                HandleNodeError(node);
            }
        }
    }
}
