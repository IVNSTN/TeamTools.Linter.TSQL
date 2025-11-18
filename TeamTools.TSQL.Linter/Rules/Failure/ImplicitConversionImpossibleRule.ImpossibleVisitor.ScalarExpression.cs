using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Scalar expression validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        // All parts of scalar computations must be compatible
        public override void Visit(BinaryExpression node)
        {
            if (lastVisitedTokenIndex >= node.LastTokenIndex)
            {
                return;
            }

            var resultingType = typeEvaluator.GetExpressionType(node.FirstExpression, node.SecondExpression);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            ValidateCanConvertAtoB(node.FirstExpression, typeEvaluator.GetExpressionType(node.FirstExpression), resultingType);
            ValidateCanConvertAtoB(node.SecondExpression, typeEvaluator.GetExpressionType(node.SecondExpression), resultingType);

            lastVisitedTokenIndex = node.LastTokenIndex;
        }

        // Left and right sides of comparison must be compatible
        public override void Visit(BooleanComparisonExpression node)
        {
            if (lastVisitedTokenIndex >= node.LastTokenIndex)
            {
                return;
            }

            var resultingType = typeEvaluator.GetExpressionType(node.FirstExpression, node.SecondExpression);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            ValidateCanConvertAtoB(node.FirstExpression, typeEvaluator.GetExpressionType(node.FirstExpression), resultingType);
            ValidateCanConvertAtoB(node.SecondExpression, typeEvaluator.GetExpressionType(node.SecondExpression), resultingType);

            lastVisitedTokenIndex = node.LastTokenIndex;
        }

        // BETWEEN is a syntax sugar for >= AND <= thus checked value must be compatible with both range limits
        public override void Visit(BooleanTernaryExpression node)
        {
            ValidateCanConvertAtoB(node.FirstExpression, node.SecondExpression);
            ValidateCanConvertAtoB(node.FirstExpression, node.ThirdExpression);
        }

        public override void Visit(InPredicate node)
        {
            if (lastVisitedTokenIndex >= node.LastTokenIndex)
            {
                return;
            }

            string resultingType = node.Values != null
                ? typeEvaluator.GetExpressionType(node.Values)
                : typeEvaluator.GetExpressionType(node.Subquery);

            // checked expression must be compatible with values resulting type
            ValidateCanConvertAtoB(node.Expression, resultingType);

            // and each value must be compatible with this type
            int n = node.Values.Count;
            for (int i = 0; i < n; i++)
            {
                ValidateCanConvertAtoB(node.Values[i], resultingType);
            }

            lastVisitedTokenIndex = node.LastTokenIndex;
        }
    }
}
