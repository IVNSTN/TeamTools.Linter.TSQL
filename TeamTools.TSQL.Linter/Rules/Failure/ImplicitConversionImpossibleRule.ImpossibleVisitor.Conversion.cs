using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Value conversions validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        private static readonly string StringFallbackType = "VARCHAR";

        // NULLIF result type is the type of the first argument
        public override void Visit(NullIfExpression node)
        {
            ValidateCanConvertAtoB(node.SecondExpression, node.FirstExpression);
        }

        // Don't check ConvertCall, TryConvertCall and so on here!
        // Explicit conversions have more options.
        // Current rule is for IMPLICIT conversions only.
        public override void Visit(TryParseCall node)
        {
            ValidateCanConvertAtoB(node.StringValue, StringFallbackType);
        }

        public override void Visit(ParseCall node)
        {
            ValidateCanConvertAtoB(node.StringValue, StringFallbackType);
        }

        // COALESCE result type is the type with highest precision out of all the input expressions
        public override void Visit(CoalesceExpression node)
        {
            var resultingType = typeEvaluator.GetExpressionType(node.Expressions);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            foreach (var e in node.Expressions)
            {
                ValidateCanConvertAtoB(e, typeEvaluator.GetExpressionType(e), resultingType);
            }
        }

        // IIF result type is the type with highest precision from THEN and ELSE expressions
        public override void Visit(IIfCall node)
        {
            var resultingType = typeEvaluator.GetExpressionType(node.ThenExpression, node.ElseExpression);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            ValidateCanConvertAtoB(node.ThenExpression, typeEvaluator.GetExpressionType(node.ThenExpression), resultingType);
            ValidateCanConvertAtoB(node.ElseExpression, typeEvaluator.GetExpressionType(node.ElseExpression), resultingType);
        }

        // ISNULL result type is the type of the first argument
        public override void Visit(FunctionCall node)
        {
            if (!node.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (node.Parameters.Count < 2)
            {
                return;
            }

            ValidateCanConvertAtoB(node.Parameters[1], node.Parameters[0]);
        }
    }
}
