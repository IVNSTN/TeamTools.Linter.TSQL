using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0985", "VAR_SELF_COMPARE")]
    internal sealed class VariableSelfCompareRule : AbstractRule
    {
        public VariableSelfCompareRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
        {
            if (!(node.FirstExpression.ExtractScalarExpression() is VariableReference leftVariable))
            {
                return;
            }

            if (!(node.SecondExpression.ExtractScalarExpression() is VariableReference rightVariable))
            {
                return;
            }

            if (leftVariable.Name.Equals(rightVariable.Name, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node.SecondExpression, leftVariable.Name);
            }
        }
    }
}
