using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0956", "SUBSTRING_TO_LIKE")]
    internal sealed class SubstringPredicateToLikeRule : AbstractRule
    {
        private readonly LikeCandidateVisitor visitor;

        public SubstringPredicateToLikeRule() : base()
        {
            visitor = new LikeCandidateVisitor(ViolationHandlerWithMessage);
        }

        public override void Visit(WhereClause node) => node.SearchCondition.AcceptChildren(visitor);

        public override void Visit(QualifiedJoin node) => node.SearchCondition.AcceptChildren(visitor);

        private class LikeCandidateVisitor : TSqlFragmentVisitor
        {
            private static readonly string MsgTemplate = "{0} -> {1}";
            private readonly Action<TSqlFragment, string> callback;

            public LikeCandidateVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(BooleanComparisonExpression node)
            {
                if (node.ComparisonType != BooleanComparisonType.Equals)
                {
                    return;
                }

                var fn = GetFunctionCallExpression(node.FirstExpression);
                var arg = node.SecondExpression;
                if (fn is null)
                {
                    fn = GetFunctionCallExpression(node.SecondExpression);
                    arg = node.FirstExpression;
                }

                if (fn is null || arg is null)
                {
                    return;
                }

                string functionName;
                if (fn is FunctionCall funcCall)
                {
                    functionName = funcCall.FunctionName.Value;

                    if (!IsFunctionCallALikeCandidate(funcCall, functionName, ref arg))
                    {
                        return;
                    }
                }
                else if (fn is LeftFunctionCall lft)
                {
                    functionName = "LEFT";

                    if (lft.Parameters.Count != 2
                    || GetScalarVarOrLiteral(lft.Parameters[1]) is null
                    || GetScalarVarOrLiteral(arg) is null)
                    {
                        // if length is unknown or dependent on column value
                        // or filter value is similarly unpredictable at compile time
                        // then it might be hard or impossible to rewrite it into
                        // understandable and performant LIKE predicate
                        return;
                    }
                }
                else
                {
                    Debug.Fail("We should never get here");
                    return;
                }

                callback(fn, string.Format(MsgTemplate, functionName, GetLikePredicate(arg)));
            }

            private static ScalarExpression GetFunctionCallExpression(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is FunctionCall fn)
                {
                    return fn;
                }

                if (node is LeftFunctionCall lft)
                {
                    return lft;
                }

                return default;
            }

            private static ScalarExpression GetScalarVarOrLiteral(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is Literal)
                {
                    return node;
                }

                if (node is VariableReference)
                {
                    return node;
                }

                return default;
            }

            private static string GetLikePredicate(ScalarExpression node)
            {
                if (node is StringLiteral str)
                {
                    return string.Format("LIKE '{0}%'", GetEscapedValue(str.Value));
                }

                if (node is VariableReference varRef)
                {
                    return string.Format("LIKE {0} + '%'", varRef.Name);
                }

                return default;
            }

            private static string GetEscapedValue(string searchString)
            {
                return searchString
                    .Replace("_", "[_]")
                    .Replace("%", "[%]");
            }

            private static bool IsFunctionCallALikeCandidate(FunctionCall funcCall, string functionName, ref ScalarExpression searchValue)
            {
                if (functionName.Equals("SUBSTRING", StringComparison.OrdinalIgnoreCase)
                && funcCall.Parameters.Count == 3)
                {
                    var stringStart = GetScalarVarOrLiteral(funcCall.Parameters[1]);
                    if (!(stringStart is IntegerLiteral stringStartPos)
                    || !int.TryParse(stringStartPos.Value, out int stringStartPosValue))
                    {
                        // unknown position in a string
                        return false;
                    }

                    searchValue = GetScalarVarOrLiteral(searchValue);

                    // Only the beginning of a string can be converted to LIKE '<value>%'
                    return stringStartPosValue == 1 && searchValue != null;
                }

                if (functionName.Equals("CHARINDEX", StringComparison.OrdinalIgnoreCase)
                && funcCall.Parameters.Count == 2)
                {
                    var charPos = GetScalarVarOrLiteral(searchValue);
                    searchValue = GetScalarVarOrLiteral(funcCall.Parameters[0]);

                    if (!(charPos is IntegerLiteral stringStartPos)
                    || !int.TryParse(stringStartPos.Value, out int expectedCharPosValue))
                    {
                        // unknown position in a string
                        return false;
                    }

                    // Only the beginning of a string can be converted to LIKE '<value>%'
                    return expectedCharPosValue == 1 && searchValue != null;
                }

                return false;
            }
        }
    }
}
