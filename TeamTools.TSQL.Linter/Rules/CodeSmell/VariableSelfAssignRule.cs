using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0984", "VAR_SELF_ASSIGN")]
    internal sealed class VariableSelfAssignRule : AbstractRule
    {
        public VariableSelfAssignRule() : base()
        {
        }

        public override void Visit(SelectSetVariable node)
        {
            if (node.AssignmentKind != AssignmentKind.Equals)
            {
                return;
            }

            ValidateSelfAssignmentExpression(node.Variable.Name, node.Expression);
        }

        public override void Visit(SetVariableStatement node)
        {
            if (node.AssignmentKind != AssignmentKind.Equals)
            {
                return;
            }

            ValidateSelfAssignmentExpression(node.Variable.Name, node.Expression);
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

        private void ValidateSelfAssignmentExpression(string leftVar, ScalarExpression node)
        {
            var rightVar = ExtractVariableFromExpression(node);

            if (string.IsNullOrEmpty(rightVar))
            {
                return;
            }

            if (leftVar.Equals(rightVar, System.StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node, leftVar);
            }
        }
    }
}
