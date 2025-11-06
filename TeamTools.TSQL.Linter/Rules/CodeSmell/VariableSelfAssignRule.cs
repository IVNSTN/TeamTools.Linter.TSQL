using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0984", "VAR_SELF_ASSIGN")]
    internal sealed class VariableSelfAssignRule : AbstractRule
    {
        public VariableSelfAssignRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var detector = new VariableSelfAssignDetector();
            node.AcceptChildren(detector);

            foreach (var badRef in detector.SelfAssignedVars)
            {
                HandleNodeError(badRef.Value, badRef.Key);
            }
        }

        private class VariableSelfAssignDetector : TSqlFragmentVisitor
        {
            public IDictionary<string, TSqlFragment> SelfAssignedVars { get; } =
                new SortedDictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

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

            private static bool IsSelfAssignment(string variableName, ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is VariableReference vref)
                {
                    return vref.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase);
                }

                if (node is ScalarSubquery sel && sel.QueryExpression is QuerySpecification spec)
                {
                    if (spec.SelectElements.Count == 1 && spec.SelectElements[0] is SelectScalarExpression sclr)
                    {
                        return IsSelfAssignment(variableName, sclr.Expression);
                    }
                }

                return false;
            }

            private void ValidateSelfAssignmentExpression(string variableName, ScalarExpression node)
            {
                if (IsSelfAssignment(variableName, node))
                {
                    SelfAssignedVars[variableName] = node;
                }
            }
        }
    }
}
