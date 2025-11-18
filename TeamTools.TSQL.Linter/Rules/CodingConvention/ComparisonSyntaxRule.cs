using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0209", "C_STYLE_OPERANDS")]
    internal sealed class ComparisonSyntaxRule : AbstractRule
    {
        private static readonly HashSet<BooleanComparisonType> ForbiddenExpressions = new HashSet<BooleanComparisonType>
        {
            BooleanComparisonType.LeftOuterJoin,
            BooleanComparisonType.RightOuterJoin,
            BooleanComparisonType.NotEqualToExclamation,
            BooleanComparisonType.NotGreaterThan,
            BooleanComparisonType.NotLessThan,
        };

        public ComparisonSyntaxRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
        {
            if (!ForbiddenExpressions.Contains(node.ComparisonType))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
