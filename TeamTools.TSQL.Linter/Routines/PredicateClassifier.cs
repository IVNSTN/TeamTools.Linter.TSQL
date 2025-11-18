using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class PredicateClassifier
    {
        // TODO : consolidate all the metadata about known built-in functions
        private static readonly HashSet<string> BuiltInPredictableFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FORMAT",
            "FORMATMESSAGE",
            "NEWID",
            "NEWSEQUENTIALID",
            "STRING_SPLIT",
        };

        private static readonly ExpressionDetailsVisitor ExpressionAnalyzer = new ExpressionDetailsVisitor(BuiltInPredictableFunctions);

        public static ExpressionElements GetExpressionElements(ScalarExpression node, HashSet<string> builtInFunctions)
        {
            return ExpressionAnalyzer.Analyze(node, builtInFunctions);
        }

        public static IEnumerable<TSqlFragment> GetNonSargablePredicates(
            ScalarExpression leftExpression,
            ScalarExpression rightExpression,
            HashSet<string> builtInFunctions)
        {
            var leftElements = GetExpressionElements(leftExpression, builtInFunctions);
            var rightElements = GetExpressionElements(rightExpression, builtInFunctions);

            // if columns not mentioned or only columns and literals used
            if ((((leftElements | rightElements) & ExpressionElements.Column) == 0)
            || (((leftElements | rightElements) ^ (ExpressionElements.Column | ExpressionElements.Literal | ExpressionElements.Variable)) == 0))
            {
                yield break;
            }

            if (leftElements.IsNonSargablePredicate())
            {
                yield return leftExpression;
            }

            if (rightElements.IsNonSargablePredicate())
            {
                yield return rightExpression;
            }
        }

        private sealed class ExpressionDetailsVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> builtInUnpredictableFunctions;
            private HashSet<string> builtInFunctions;

            public ExpressionDetailsVisitor(HashSet<string> builtInUnpredictableFunctions)
            {
                this.builtInUnpredictableFunctions = builtInUnpredictableFunctions;
            }

            public ExpressionElements Flags { get; private set; }

            public ExpressionElements Analyze(ScalarExpression expression, HashSet<string> builtInFunctions)
            {
                this.builtInFunctions = builtInFunctions;
                Flags = 0;
                expression.Accept(this);
                return Flags;
            }

            public override void Visit(FunctionCall node)
            {
                if (builtInUnpredictableFunctions.Contains(node.FunctionName.Value))
                {
                    Flags |= ExpressionElements.UserDefinedFunction;
                    return;
                }

                if (builtInFunctions.Contains(node.FunctionName.Value))
                {
                    Flags |= ExpressionElements.BuiltInFunction;
                    return;
                }

                Flags |= ExpressionElements.UserDefinedFunction;
            }

            public override void Visit(ColumnReferenceExpression node)
            {
                Flags |= ExpressionElements.Column;
            }

            public override void Visit(NullIfExpression node)
            {
                Flags |= ExpressionElements.Computation;
            }

            public override void Visit(CastCall node)
            {
                Flags |= ExpressionElements.BuiltInFunction;
            }

            public override void Visit(TryCastCall node)
            {
                Flags |= ExpressionElements.BuiltInFunction;
            }

            public override void Visit(ConvertCall node)
            {
                Flags |= ExpressionElements.BuiltInFunction;
            }

            public override void Visit(TryConvertCall node)
            {
                Flags |= ExpressionElements.BuiltInFunction;
            }

            public override void Visit(IIfCall node)
            {
                Flags |= ExpressionElements.BuiltInFunction;
            }

            public override void Visit(CaseExpression node)
            {
                Flags |= ExpressionElements.Computation;
            }

            public override void Visit(Literal node)
            {
                Flags |= ExpressionElements.Literal;
            }

            public override void Visit(VariableReference node)
            {
                Flags |= ExpressionElements.Variable;
            }

            public override void Visit(BinaryExpression node)
            {
                Flags |= ExpressionElements.Computation;
            }
        }
    }
}
