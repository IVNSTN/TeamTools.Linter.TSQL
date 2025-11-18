using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0285", "PROC_PARAMS_PARENTHESIS")]
    internal sealed class ProcedureParamsParenthesisRule : AbstractRule
    {
        public ProcedureParamsParenthesisRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            int firstToken = node.ProcedureReference.LastTokenIndex;
            int lastToken;
            if (node.Parameters.Count > 0)
            {
                lastToken = node.Parameters[0].FirstTokenIndex;
            }
            else if (node.Options.Count > 0)
            {
                lastToken = node.Options[0].FirstTokenIndex;
            }
            else if (node.StatementList?.Statements.Count > 0)
            {
                lastToken = node.StatementList.Statements[0].FirstTokenIndex;
            }
            else
            {
                lastToken = node.LastTokenIndex;
            }

            int i = firstToken;
            bool parenthesisFound = false;
            while (i < lastToken && !parenthesisFound)
            {
                parenthesisFound = node.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis;
                i++;
            }

            if (!parenthesisFound)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
