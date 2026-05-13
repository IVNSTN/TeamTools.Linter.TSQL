using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0893", "ISOLATION_CONTRADICTION")]
    internal sealed partial class IsolationLevelContradictionRule : AbstractRule
    {
        public IsolationLevelContradictionRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                proc.StatementList?.Accept(MakeVisitor());
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                trg.StatementList?.Accept(MakeVisitor());
            }
            else
            {
                // no proc/trigger/func - validating this ad-hoc batch
                batch.AcceptChildren(MakeVisitor());
            }
        }

        private TSqlFragmentVisitor MakeVisitor()
        {
            return new IsolationLevelSwitchVisitor(ViolationHandlerWithMessage);
        }
    }
}
