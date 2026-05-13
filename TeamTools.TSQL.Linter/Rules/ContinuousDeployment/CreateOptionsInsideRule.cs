using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0895", "CREATE_OPTIONS_INSIDE")]
    internal sealed class CreateOptionsInsideRule : AbstractRule
    {
        public CreateOptionsInsideRule() : base()
        {
        }

        public override void Visit(PredicateSetStatement node)
        {
            if (node.Options.HasFlag(SetOptions.AnsiNulls))
            {
                HandleNodeError(node, "ANSI_NULLS");
            }
            else if (node.Options.HasFlag(SetOptions.QuotedIdentifier))
            {
                HandleNodeError(node, "QUOTED_IDENTIFIER");
            }
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                proc.StatementList?.AcceptChildren(this);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                trg.StatementList?.AcceptChildren(this);
            }
        }
    }
}
