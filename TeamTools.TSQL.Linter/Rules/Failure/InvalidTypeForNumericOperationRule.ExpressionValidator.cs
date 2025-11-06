using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// ExpressionValidator implementation.
    /// </summary>
    internal partial class InvalidTypeForNumericOperationRule
    {
        private class ExpressionValidator : TSqlFragmentVisitor
        {
            private static readonly string MsgTemplate = "{1} on {0} type";
            private readonly IDictionary<string, string> knownTypes;
            private readonly ExpressionResultTypeEvaluator evaluator;
            private readonly Action<TSqlFragment, string> callback;

            public ExpressionValidator(
                IDictionary<string, string> knownTypes,
                ExpressionResultTypeEvaluator evaluator,
                Action<TSqlFragment, string> callback)
            {
                this.knownTypes = knownTypes;
                this.callback = callback;
                this.evaluator = evaluator;
            }

            public override void Visit(UnaryExpression node)
            {
                ValidateExpression(node.Expression, UnaryExpressionTypeToBitwise(node.UnaryExpressionType));
            }

            public override void Visit(BinaryExpression node)
            {
                ValidateExpression(node.FirstExpression, node.BinaryExpressionType);
                ValidateExpression(node.SecondExpression, node.BinaryExpressionType);
            }

            public override void Visit(SetVariableStatement node)
            {
                if (node.AssignmentKind == AssignmentKind.Equals)
                {
                    return;
                }

                var operatorType = AssignmentKindToBinary(node.AssignmentKind);

                ValidateExpression(node.Variable, operatorType);
                ValidateExpression(node.Expression, operatorType);
            }

            private static BinaryExpressionType AssignmentKindToBinary(AssignmentKind operatorType)
            {
                switch (operatorType)
                {
                    case AssignmentKind.MultiplyEquals:
                        return BinaryExpressionType.Multiply;

                    case AssignmentKind.ModEquals:
                        return BinaryExpressionType.Modulo;

                    case AssignmentKind.DivideEquals:
                        return BinaryExpressionType.Divide;

                    case AssignmentKind.SubtractEquals:
                        return BinaryExpressionType.Subtract;

                    case AssignmentKind.BitwiseAndEquals:
                        return BinaryExpressionType.BitwiseAnd;

                    case AssignmentKind.BitwiseOrEquals:
                        return BinaryExpressionType.BitwiseOr;

                    case AssignmentKind.BitwiseXorEquals:
                        return BinaryExpressionType.BitwiseXor;

                    default:
                        // as default which will be ignored
                        return BinaryExpressionType.Add;
                }
            }

            private static BinaryExpressionType UnaryExpressionTypeToBitwise(UnaryExpressionType operatorType)
            {
                switch (operatorType)
                {
                    case UnaryExpressionType.Negative:
                        return BinaryExpressionType.Subtract;

                    case UnaryExpressionType.BitwiseNot:
                        return BinaryExpressionType.BitwiseXor;

                    default:
                        return BinaryExpressionType.Add;
                }
            }

            // TODO : some refactoring needed. too much local magic.
            private bool IsValidExpression(string expressionType, BinaryExpressionType operatorType)
            {
                if (string.IsNullOrEmpty(expressionType))
                {
                    return true;
                }

                if (!knownTypes.ContainsKey(expressionType))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(knownTypes[expressionType]))
                {
                    // all operators allowed
                    return true;
                }

                if (knownTypes[expressionType].Equals("NONE", StringComparison.OrdinalIgnoreCase))
                {
                    // specific methods required to modify value of this type
                    // or modification is impossible, only reassignment allowed
                    return false;
                }

                if (operatorType == BinaryExpressionType.Add)
                {
                    // most of types can handle +
                    return true;
                }

                if (operatorType.In(BinaryExpressionType.BitwiseAnd, BinaryExpressionType.BitwiseOr, BinaryExpressionType.BitwiseXor))
                {
                    return knownTypes[expressionType].Equals("BITWISE");
                }

                if (knownTypes[expressionType].Equals("MATH", StringComparison.OrdinalIgnoreCase))
                {
                    // numeric non-int types can do all math except bitwise operations
                    return true;
                }

                if (operatorType == BinaryExpressionType.Subtract
                && knownTypes[expressionType].Equals("SUBTRACT", StringComparison.OrdinalIgnoreCase))
                {
                    // datetime can handle -
                    return true;
                }

                return false;
            }

            private void ValidateExpression(ScalarExpression node, BinaryExpressionType operatorType)
            {
                string expressionType = evaluator.GetExpressionType(node);

                if (IsValidExpression(expressionType, operatorType))
                {
                    return;
                }

                callback(node, string.Format(MsgTemplate, expressionType, operatorType.ToString()));
            }
        }
    }
}
