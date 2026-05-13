using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExtractExpressionRule
    {
        private sealed class ComplexExpressionVisitor : VisitorWithCallback
        {
            // TODO : load all built-in functions from metadata?
            private static readonly HashSet<string> IgnorableFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ERROR_LINE",
                "ERROR_MESSAGE",
                "ERROR_NUMBER",
                "ERROR_PROCEDURE",
                "ERROR_SEVERITY",
                "ERROR_STATE",
                "GETDATE",
                "GETUTCDATE",
                "ISNULL",
                "SYSDATETIME",
            };

            private readonly HashSet<string> complexExpressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public ComplexExpressionVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            // Explicit - to avoid unnecessary nested expression processing (see Validate details)
            public override void ExplicitVisit(BinaryExpression node) => Validate(node);

            public override void ExplicitVisit(FunctionCall node) => Validate(node);

            public override void ExplicitVisit(SearchedCaseExpression node) => Validate(node);

            public override void ExplicitVisit(SimpleCaseExpression node) => Validate(node);

            // Derived subqueries have different scope and their expressions should be ignored
            public override void ExplicitVisit(ScalarSubquery node)
            { }

            public override void ExplicitVisit(QueryDerivedTable node)
            { }

            private static ScalarExpression DetectComplexExpression(ScalarExpression src)
            {
                if (src is null)
                {
                    return default;
                }

                while (src is ParenthesisExpression pe)
                {
                    src = pe.Expression;
                }

                // Some functions seem to be trivial enough.
                // Also some built-in functions are not derived from FunctionCall (e.g. CastCall)
                // and will be ignored as well.
                if (src is FunctionCall func
                && !IgnorableFunctions.Contains(func.FunctionName.Value))
                {
                    return func;
                }

                // Except trivial, very short expressions
                if (src is BinaryExpression bin
                && (bin.LastTokenIndex - bin.FirstTokenIndex) > 5)
                {
                    return bin;
                }

                if (src is SimpleCaseExpression simpleCase
                && simpleCase.WhenClauses.Count > 1)
                {
                    return simpleCase;
                }

                if (src is SearchedCaseExpression searchCase
                && searchCase.WhenClauses.Count > 1)
                {
                    return searchCase;
                }

                return default;
            }

            private void Validate(ScalarExpression node)
            {
                // If external (more complex) expression can be reused then no need to go deeper
                // to internal (less complex) nested expressions
                if (ExpressionContainsExtractableParts(node))
                {
                    node.AcceptChildren(this);
                }
            }

            private bool ExpressionContainsExtractableParts(ScalarExpression expr)
            {
                var complexExpression = DetectComplexExpression(expr);
                if (complexExpression is null)
                {
                    // Not a complex expression
                    return false;
                }

                if (complexExpressions.Add(complexExpression.GetFragmentCleanedText()))
                {
                    // Expression is complex but new - going deeper
                    return true;
                }

                Callback(expr);

                // Expression is complex but it is a dup and should be extracted
                // no need to analyze nested expressions
                return false;
            }
        }
    }
}
