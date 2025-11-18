using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

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
            string leftVariable = ExtractVariableFromExpression(node.FirstExpression);
            if (string.IsNullOrEmpty(leftVariable))
            {
                return;
            }

            string rightVariable = ExtractVariableFromExpression(node.SecondExpression);
            if (string.IsNullOrEmpty(rightVariable))
            {
                return;
            }

            if (leftVariable.Equals(rightVariable, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node.SecondExpression, leftVariable);
            }
        }

        private static string ExtractVariableFromExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is VariableReference vref)
            {
                return vref.Name;
            }

            if (node is ScalarSubquery sel && sel.QueryExpression is QuerySpecification spec)
            {
                if (spec.SelectElements.Count == 1 && spec.SelectElements[0] is SelectScalarExpression sclr)
                {
                    return ExtractVariableFromExpression(sclr.Expression);
                }
            }

            return default;
        }
    }
}
