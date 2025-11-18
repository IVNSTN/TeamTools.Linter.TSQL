using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0956", "SUBSTRING_TO_LIKE")]
    internal sealed class SubstringPredicateToLikeRule : AbstractRule
    {
        public SubstringPredicateToLikeRule() : base()
        {
        }

        public override void Visit(WhereClause node)
        {
            var visitor = new LikeCandidateVisitor(ViolationHandlerWithMessage);
            node.AcceptChildren(visitor);
        }

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

                if (fn is null)
                {
                    return;
                }

                arg = GetScalarVarOrStringLiteral(arg);

                if (arg is null)
                {
                    return;
                }

                string functionName;
                if (fn is FunctionCall funcCall)
                {
                    functionName = funcCall.FunctionName.Value;

                    if (!functionName.Equals("SUBSTRING", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    // LEFT function always works from the start
                    if (funcCall != null
                    && functionName.Equals("SUBSTRING", StringComparison.OrdinalIgnoreCase)
                    && funcCall.Parameters.Count == 3)
                    {
                        var stringStart = GetScalarVarOrStringLiteral(funcCall.Parameters[1]);
                        if (stringStart is null
                        || !(stringStart is IntegerLiteral stringStartPos)
                        || !int.TryParse(stringStartPos.Value, out int stringStartPosValue))
                        {
                            // unknown position in a string
                            return;
                        }

                        if (stringStartPosValue > 1)
                        {
                            // not the beginning of a string
                            return;
                        }
                    }
                }
                else
                {
                    functionName = "LEFT";
                }

                callback(fn, string.Format(MsgTemplate, functionName, GetLikePredicate(arg)));
            }

            private static ScalarExpression GetFunctionCallExpression(ScalarExpression node)
            {
                if (node is ParenthesisExpression pe)
                {
                    return GetFunctionCallExpression(pe.Expression);
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

            private static ScalarExpression GetScalarVarOrStringLiteral(ScalarExpression node)
            {
                if (node is ParenthesisExpression pe)
                {
                    return GetScalarVarOrStringLiteral(pe.Expression);
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
        }
    }
}
