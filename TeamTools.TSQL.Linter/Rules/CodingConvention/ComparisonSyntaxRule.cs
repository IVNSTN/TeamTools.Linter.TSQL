using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0209", "C_STYLE_OPERANDS")]
    internal sealed class ComparisonSyntaxRule : AbstractRule
    {
        private static readonly Lazy<IList<BooleanComparisonType>> ForbiddenExpressionsInstance
            = new Lazy<IList<BooleanComparisonType>>(() => InitForbiddenExpressionsInstance(), true);

        public ComparisonSyntaxRule() : base()
        {
        }

        private static IList<BooleanComparisonType> ForbiddenExpressions => ForbiddenExpressionsInstance.Value;

        public override void Visit(BooleanComparisonExpression node)
        {
            if (!ForbiddenExpressions.Contains(node.ComparisonType))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static IList<BooleanComparisonType> InitForbiddenExpressionsInstance()
        {
            return new List<BooleanComparisonType>
            {
                BooleanComparisonType.LeftOuterJoin,
                BooleanComparisonType.RightOuterJoin,
                BooleanComparisonType.NotEqualToExclamation,
                BooleanComparisonType.NotGreaterThan,
                BooleanComparisonType.NotLessThan,
            };
        }
    }
}
