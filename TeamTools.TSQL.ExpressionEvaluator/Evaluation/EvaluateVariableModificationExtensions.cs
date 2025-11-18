using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateVariableModificationExtensions
    {
        public static SqlValue EvaluateVarModification(
            this SqlExpressionEvaluator eval,
            string varName,
            ScalarExpression expr,
            AssignmentKind operKind)
        {
            var old = eval.VariableEvaluator.GetValueAt(varName, expr.FirstTokenIndex);
            if (old is null)
            {
                // FIXME : register all vars and their assignments first
                // no matter if type is currently unsupported
                // eval.Violations.RegisterViolation(new VariableReferencedBeforeAssignment(varName, expr.FirstTokenIndex));
                // TODO : maybe SqlValueKind.Null ? or unknown based on expession evaluation?
                return default;
            }

            var ev = eval.Converter.ImplicitlyConvertTo(
                eval.VariableEvaluator.GetVariableTypeReference(varName),
                eval.EvaluateExpression(expr));

            return eval.EvaluateFormula(old, ev, ConvertToOperationType(operKind), expr);
        }

        public static BinaryExpressionType ConvertToOperationType(AssignmentKind assignOperKind)
        {
            switch (assignOperKind)
            {
                case AssignmentKind.AddEquals: return BinaryExpressionType.Add;

                case AssignmentKind.ConcatEquals: return BinaryExpressionType.Add;

                case AssignmentKind.SubtractEquals: return BinaryExpressionType.Subtract;

                case AssignmentKind.MultiplyEquals: return BinaryExpressionType.Multiply;

                case AssignmentKind.DivideEquals: return BinaryExpressionType.Divide;

                case AssignmentKind.ModEquals: return BinaryExpressionType.Modulo;

                // TODO : or fail?
                default: return BinaryExpressionType.LeftShift;
            }
        }

        public static void ProcessVariableAssignment(
            this SqlScriptAnalyzer scriptAnalyzer,
            string varName,
            ScalarExpression expr,
            AssignmentKind operKind)
        {
            // TODO : support XML methods
            if (expr is null)
            {
                // XML.modify
                return;
            }

            SqlValue ev;

            if (operKind == AssignmentKind.Equals)
            {
                // @var =
                ev = scriptAnalyzer.Evaluator.EvaluateExpression(expr);
            }
            else
            {
                // @var +=, -= etc.
                ev = scriptAnalyzer.Evaluator.EvaluateVariableModification(varName, expr, operKind);
            }

            if (ev is null)
            {
                scriptAnalyzer.VarRegistry.RegisterEvaluatedValue(varName, expr.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, expr));
            }
            else
            {
                // TODO : detect variable self-assignement and report corresponding violation
                ev.Source = new SqlValueSource(ev.SourceKind, expr);
                scriptAnalyzer.VarRegistry.RegisterEvaluatedValue(varName, expr.LastTokenIndex, ev);
            }
        }
    }
}
