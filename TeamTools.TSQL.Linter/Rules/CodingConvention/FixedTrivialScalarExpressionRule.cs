using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0294", "FIXED_TRIVIAL_SCALAR")]
    internal sealed class FixedTrivialScalarExpressionRule : AbstractRule
    {
        public FixedTrivialScalarExpressionRule() : base()
        {
        }

        public override void Visit(ExistsPredicate node)
        {
            var spec = node.Subquery.QueryExpression.GetQuerySpecification();
            if (spec is null || spec.SelectElements.Count != 1)
            {
                return;
            }

            if (spec.SelectElements[0] is SelectScalarExpression e && e.ColumnName is null)
            {
                ValidateSelectExpression(e.Expression, true);
            }
        }

        public override void Visit(FunctionCall node)
        {
            if (!node.FunctionName.Value.Equals("COUNT", StringComparison.OrdinalIgnoreCase)
            || node.Parameters.Count != 1)
            {
                return;
            }

            ValidateSelectExpression(node.Parameters[0]);
        }

        private static bool IsEventuallyScalarLiteralExpression(ScalarExpression expr)
        {
            if (expr is ParenthesisExpression p)
            {
                return IsEventuallyScalarLiteralExpression(p.Expression);
            }
            else if (expr is ScalarSubquery q && q.QueryExpression is QuerySpecification qs
            && qs.WhereClause is null && qs.SelectElements.Count == 1
            && qs.SelectElements[0] is SelectScalarExpression sse)
            {
                return IsEventuallyScalarLiteralExpression(sse.Expression);
            }

            return expr is Literal;
        }

        private static bool IsValidExpression(ScalarExpression expr, bool forceLiteralOnly = false)
        {
            if (expr is IntegerLiteral l)
            {
                return int.TryParse(l.Value, out int literalValue) && literalValue == 1;
            }

            if (forceLiteralOnly)
            {
                return false;
            }

            // if eventually literal but written in a verbose way - just use valid literal
            // otherwise if there is something complex - literal might mean something incompatible
            // thus not forcing to rewriting
            return !IsEventuallyScalarLiteralExpression(expr);
        }

        private void ValidateSelectExpression(ScalarExpression expr, bool forceLiteralOnly = false)
        {
            // COUNT(*) is bad as well as non regular column reference or "1"
            if ((expr is ColumnReferenceExpression col && col.ColumnType == ColumnType.Wildcard)
            || !IsValidExpression(expr, forceLiteralOnly))
            {
                HandleNodeError(expr);
            }
        }
    }
}
