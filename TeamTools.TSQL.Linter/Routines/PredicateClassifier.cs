using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class PredicateClassifier
    {
        public static ExpressionElements GetExpressionElements(ScalarExpression node, ICollection<string> builtInFunctions)
        {
            var exprVisitor = new ExpressionDetailsVisitor(
                builtInFunctions,
                // TODO : consolidate all the metadata about known built-in functions
                new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "FORMAT",
                    "FORMATMESSAGE",
                    "STRING_SPLIT",
                    "NEWID",
                    "NEWSEQUENTIALID",
                });

            node.Accept(exprVisitor);
            return exprVisitor.Flags;
        }

        public static IEnumerable<TSqlFragment> GetNonSargablePredicates(
            ScalarExpression leftExpression,
            ScalarExpression rightExpression,
            ICollection<string> builtInFunctions)
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

            yield break;
        }

        private class ExpressionDetailsVisitor : TSqlFragmentVisitor
        {
            private readonly ICollection<string> builtInFunctions;
            private readonly ICollection<string> builtInUnpredictableFunctions;

            public ExpressionDetailsVisitor(ICollection<string> builtInFunctions, ICollection<string> builtInUnpredictableFunctions)
            {
                this.builtInFunctions = builtInFunctions;
                this.builtInUnpredictableFunctions = builtInUnpredictableFunctions;
            }

            public ExpressionElements Flags { get; private set; }

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
