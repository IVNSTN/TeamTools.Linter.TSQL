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

        public override void Visit(FunctionStatementBody node)
        {
            if (!(node.ReturnType is SelectFunctionReturnType sel))
            {
                // only inline functions must be handled
                return;
            }

            if (sel.SelectStatement.QueryExpression.OrderByClause == null)
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
    }
}
