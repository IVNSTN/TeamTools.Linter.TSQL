using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Impossible conversion detector for ImplicitConversionImpossibleRule.
    /// </summary>
    // TODO : high-level concept is very similar to ImplicitTruncationRule
    internal partial class ImpossibleVisitor : TSqlFragmentVisitor
    {
        private static readonly string CallbackMsgTemplate = "{0} -> {1}";
        private readonly Dictionary<string, HashSet<string>> impossibleTypeConversions;
        private readonly HashSet<string> noImplicitConversionTypes;
        private readonly ExpressionResultTypeEvaluator typeEvaluator;
        private readonly TSqlParser parser;
        private readonly Action<TSqlFragment, string> callback;
        private int lastVisitedTokenIndex = -1;

        public ImpossibleVisitor(
            Dictionary<string, HashSet<string>> impossibleTypeConversions,
            HashSet<string> noImplicitConversionTypes,
            ExpressionResultTypeEvaluator typeEvaluator,
            TSqlParser parser,
            Action<TSqlFragment, string> callback)
        {
            this.impossibleTypeConversions = impossibleTypeConversions;
            this.noImplicitConversionTypes = noImplicitConversionTypes;
            this.typeEvaluator = typeEvaluator;
            this.parser = parser;
            this.callback = callback;
        }

        private void ValidateCanConvertAtoB(ScalarExpression expressionA, ScalarExpression expressionB)
        {
            ValidateCanConvertAtoB(expressionA, typeEvaluator.GetExpressionType(expressionA), typeEvaluator.GetExpressionType(expressionB));
        }

        private void ValidateCanConvertAtoB(ScalarExpression expression, string typeName)
        {
            ValidateCanConvertAtoB(expression, typeEvaluator.GetExpressionType(expression), typeName);
        }

        private void ValidateCanConvertAtoB(string typeName, ScalarExpression expression)
        {
            ValidateCanConvertAtoB(expression, typeName, typeEvaluator.GetExpressionType(expression));
        }

        private void ValidateCanConvertAtoB(TSqlFragment node, string typeA, string typeB)
        {
            if (string.IsNullOrEmpty(typeA) || string.IsNullOrEmpty(typeB))
            {
                return;
            }

            if (string.Equals(typeA, typeB, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (noImplicitConversionTypes.Contains(typeA) || typeB == "CURSOR")
            {
                callback(node, string.Format(CallbackMsgTemplate, typeA, typeB));
                return;
            }

            if (!impossibleTypeConversions.TryGetValue(typeA, out var restrictions))
            {
                return;
            }

            if (!restrictions.Contains(typeB))
            {
                return;
            }

            callback(node, string.Format(CallbackMsgTemplate, typeA, typeB));
        }
    }
}
