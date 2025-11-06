using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    public class MultiAssignDetector
    {
        private readonly ICollection<string> assignedVars = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly IViolationRegistrar violations;

        public MultiAssignDetector(IViolationRegistrar violations)
        {
            this.violations = violations;
        }

        public static void Monitor(IViolationRegistrar violations, Action<Action<string, TSqlFragment>> codeBlock)
        {
            var instance = new MultiAssignDetector(violations);

            codeBlock.Invoke(instance.VarAssign);
        }

        private static bool ContainsVariableSelfReference(string varName, TSqlFragment node)
        {
            return node.EnumElements()
                .OfType<VariableReference>()
                .Any(vr => string.Equals(varName, vr.Name, StringComparison.OrdinalIgnoreCase));
        }

        private void VarAssign(string varName, TSqlFragment node)
        {
            if (!assignedVars.TryAddUnique(varName))
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
