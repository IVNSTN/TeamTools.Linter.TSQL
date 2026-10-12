using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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
                // ignore += and such
                return;
            }

            ValidateSelfAssignmentExpression(node.Variable.Name, node.Expression);
        }

        public override void Visit(SetVariableStatement node)
        {
            if (node.AssignmentKind != AssignmentKind.Equals)
            {
                // ignore += and such
                return;
            }

            ValidateSelfAssignmentExpression(node.Variable.Name, node.Expression);
        }

        private void ValidateSelfAssignmentExpression(string leftVar, ScalarExpression node)
        {
            if (!(node.ExtractScalarExpression() is VariableReference rightVar))
            {
                return;
            }

            if (leftVar.Equals(rightVar.Name, System.StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(rightVar, leftVar);
            }
        }
    }
}
