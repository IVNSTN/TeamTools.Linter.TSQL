using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0145", "TRIGGER_SET_NOCOUNT")]
    [TriggerRule]
    internal sealed class NocountOptionInTriggerRule : AbstractRule
    {
        public NocountOptionInTriggerRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                ValidateBody(trg);
            }
        }

        private void ValidateBody(TriggerStatementBody node)
        {
            if (node.Options.HasOption(TriggerOptionKind.NativeCompile))
            {
                // natively compiled triggers cannot have SET NOCOUNT clause
                return;
            }

            var nocountVisitor = new SetOptionsVisitor();
            node.AcceptChildren(nocountVisitor);

            // Nocount was set to ON
            if (nocountVisitor.DetectedOptions.TryGetValue(SetOptions.NoCount, out var nocountState)
            && (nocountState ?? false))
            {
                return;
            }

            HandleNodeError(node.StatementList.GetFirstStatement());
        }
    }
}
