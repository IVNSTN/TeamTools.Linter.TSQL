using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0732", "GO_AFTER_PROC")]
    internal sealed class GoAfterProcRule : AbstractRule
    {
        public GoAfterProcRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            if ((node.StatementList?.Statements?.Count ?? 0) <= 1)
            {
                // No body or single top-level statement (maybe BEGIN-END)
                return;
            }

            if (node.StatementList.Statements[0] is BeginEndBlockStatement)
            {
                // If the first one is not BEGIN-END then it may be
                // a proc without top-level begin-end. Such cases are controlled
                // by separate rule.
                HandleNodeError(node.StatementList.Statements[1]);
            }
        }
    }
}
