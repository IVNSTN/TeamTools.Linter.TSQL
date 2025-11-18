using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateVariableReferenceExtensions
    {
        public static SqlValue EvaluateVariable(this SqlExpressionEvaluator eval, VariableReference varRef)
        {
            var ev = eval.VariableEvaluator.GetValueAt(varRef.Name, varRef.FirstTokenIndex);
            if (ev is null)
            {
                // FIXME : if this is ISNULL or COALESCE first arg
                // then no violation should be reported
                // FIXME : register all vars and their assignments first
                // no matter if type is currently unsupported
                // eval.Violations.RegisterViolation(new VariableReferencedBeforeAssignment(varRef.Name, varRef.FirstTokenIndex));
                // FIXME : if var was never assigned for sure, then MakeNullValue() should be called
                ev = eval.VariableEvaluator.GetVariableTypeReference(varRef.Name)?.MakeUnknownValue();
            }

            return ev;
        }
    }
}
