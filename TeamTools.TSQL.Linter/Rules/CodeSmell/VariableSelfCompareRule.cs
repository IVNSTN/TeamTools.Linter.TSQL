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

        public override void Visit(TSqlBatch node)
        => node.AcceptChildren(new VariableSelfCompareDetector(HandleNodeError));

        private class VariableSelfCompareDetector : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;

            public VariableSelfCompareDetector(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(BooleanComparisonExpression node)
            {
                string leftVariable = ExtractVariableFromExpression(node.FirstExpression);
                string rightVariable = ExtractVariableFromExpression(node.SecondExpression);

                if (!string.IsNullOrEmpty(leftVariable) && leftVariable.Equals(rightVariable, StringComparison.OrdinalIgnoreCase))
                {
                    callback(node.SecondExpression, leftVariable);
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

                return "";
            }
        }
    }
}
