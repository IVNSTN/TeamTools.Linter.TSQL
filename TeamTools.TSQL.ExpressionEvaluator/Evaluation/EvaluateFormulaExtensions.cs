using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateFormulaExtensions
    {
        // TODO : evaluate result type before any computations
        public static SqlValue EvaluateFormula(this SqlExpressionEvaluator eval, BinaryExpression expr)
        {
            var a = eval.EvaluateExpression(expr.FirstExpression);
            var b = eval.EvaluateExpression(expr.SecondExpression);

            return eval.EvaluateFormula(a, b, expr.BinaryExpressionType, expr);
        }

        public static SqlValue EvaluateFormula(this SqlExpressionEvaluator eval, SqlValue a, SqlValue b, BinaryExpressionType operKind, TSqlFragment node = null)
        {
            if (a is null || b is null)
            {
                return default;
            }

            // FIXME: for VARCHAR(1) + VARCHAR(4) it returns VARCHAR(30) instead of VARCHAR(5)
            var outputType = eval.Converter.EvaluateOutputType(a, b);

            if (string.IsNullOrEmpty(outputType))
            {
                return default;
            }

            a = eval.Converter.ImplicitlyConvertTo(outputType, a);
            b = eval.Converter.ImplicitlyConvertTo(outputType, b);

            if (a is null || b is null)
            {
                // unsupported types or values are incompatible
                return default;
            }

            SqlValue ev;

            // implicit conversion must happen before returning values
            if (a.IsNull)
            {
                eval.RegisterNullArithmeticsViolation(a, node);

                // "a" has already been converted to outputType so this is fine
                ev = a;
            }
            else if (b.IsNull)
            {
                eval.RegisterNullArithmeticsViolation(b, node);

                // "b" has already been converted to outputType so this is fine
                ev = b;
            }
            else
            {
                ev = DoComputeFormula(a, b, operKind);
            }

            if (ev != null)
            {
                var sourceKind = SqlValueSourceKind.Expression;

                if (ev.IsPreciseValue
                && a.SourceKind == SqlValueSourceKind.Literal && b.SourceKind == SqlValueSourceKind.Literal)
                {
                    sourceKind = SqlValueSourceKind.Literal;
                }

                ev.Source = new SqlValueSource(sourceKind, node ?? a.Source.Node);
            }

            return ev;
        }

        private static void RegisterNullArithmeticsViolation(this SqlExpressionEvaluator eval, SqlValue value, TSqlFragment node)
        {
            eval.Violations.RegisterViolation(new NullArithmeticsViolation(value.Source.ToString(), new SqlValueSource(SqlValueSourceKind.Expression, node)));
        }

        private static SqlValue DoComputeFormula(SqlValue a, SqlValue b, BinaryExpressionType operKind)
        {
            var typeHandler = a.GetTypeHandler();

            switch (operKind)
            {
                case BinaryExpressionType.Add:
                    {
                        if (typeHandler is IPlusOperatorHandler oper)
                        {
                            return oper.Sum(a, b);
                        }

                        break;
                    }

                case BinaryExpressionType.Subtract:
                    {
                        if (typeHandler is IMinusOperatorHandler oper)
                        {
                            return oper.Subtract(a, b);
                        }

                        break;
                    }

                case BinaryExpressionType.Multiply:
                    {
                        if (typeHandler is IMultiplyOperatorHandler oper)
                        {
                            return oper.Multiply(a, b);
                        }

                        break;
                    }

                case BinaryExpressionType.Divide:
                    {
                        if (typeHandler is IDivideOperatorHandler oper)
                        {
                            return oper.Divide(a, b);
                        }

                        break;
                    }
            }

            // TODO : register violation?
            return default;
        }
    }
}
