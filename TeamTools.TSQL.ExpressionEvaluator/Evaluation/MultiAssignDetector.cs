using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    public class MultiAssignDetector
    {
        private readonly HashSet<string> assignedVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly IViolationRegistrar violations;
        private readonly Action<string, TSqlFragment> varAssignValidator;

        public MultiAssignDetector(IViolationRegistrar violations)
        {
            this.violations = violations;
            varAssignValidator = new Action<string, TSqlFragment>(VarAssign);
        }

        public static void Monitor(IViolationRegistrar violations, Action<Action<string, TSqlFragment>> codeBlock)
        {
            var instance = new MultiAssignDetector(violations);

            codeBlock.Invoke(instance.varAssignValidator);
        }

        private static bool ContainsVariableSelfReference(string varName, TSqlFragment node)
        {
            foreach (var vr in node.EnumElements().OfType<VariableReference>())
            {
                if (string.Equals(varName, vr.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void VarAssign(string varName, TSqlFragment node)
        {
            if (!assignedVars.Add(varName))
            {
                AlreadyAssigned(varName, node);
            }
        }

        private void AlreadyAssigned(string varName, TSqlFragment node)
        {
            if (!ContainsVariableSelfReference(varName, node))
            {
                violations.RegisterViolation(new AmbiguousVariableMultipleAssignment(varName, new SqlValueSource(SqlValueSourceKind.Expression, node)));
            }
        }
    }
}
