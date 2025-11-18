using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0951", "FN_SORTED_OUTPUT")]
    internal sealed class SortedOutputFromFunctionRule : AbstractRule
    {
        public SortedOutputFromFunctionRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private static bool IsTopOne(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is IntegerLiteral lit)
            {
                return int.TryParse(lit.Value, out int literalValue)
                    && literalValue == 1;
            }

            return false;
        }

        private void DoValidate(FunctionStatementBody node)
        {
            if (!(node.ReturnType is SelectFunctionReturnType sel))
            {
                // only inline functions must be handled
                return;
            }

            if (sel.SelectStatement.QueryExpression.OrderByClause is null)
            {
                return;
            }

            if (sel.SelectStatement.QueryExpression.OffsetClause != null)
            {
                // offset fetch needs order by
                return;
            }

            var top = sel.SelectStatement.QueryExpression.GetQuerySpecification()?.TopRowFilter;
            if (top != null && IsTopOne(top.Expression))
            {
                // top 1 is ok
                return;
            }

            HandleNodeError(sel.SelectStatement.QueryExpression.OrderByClause);
        }
    }
}
