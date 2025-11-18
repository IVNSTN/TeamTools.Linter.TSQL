using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0250", "REDUNDANT_OPERATOR")]
    internal sealed class UnaryOperatorRepeatedRule : AbstractRule
    {
        public UnaryOperatorRepeatedRule() : base()
        {
        }

        // TODO : avoid double visiting nested expressions
        public override void Visit(BooleanExpression node) => ValidateOperators(node);

        public override void Visit(ScalarExpression node) => ValidateOperators(node);

        private static bool IsOperator(TSqlTokenType tokenType)
        {
            return tokenType == TSqlTokenType.Plus
                || tokenType == TSqlTokenType.Minus
                || tokenType == TSqlTokenType.EqualsSign
                || tokenType == TSqlTokenType.Star
                || tokenType == TSqlTokenType.Not
                || tokenType == TSqlTokenType.And
                || tokenType == TSqlTokenType.Or
                || tokenType == TSqlTokenType.Tilde
                || tokenType == TSqlTokenType.Bang
                || tokenType == TSqlTokenType.GreaterThan
                || tokenType == TSqlTokenType.LessThan;
        }

        private void ValidateOperators<T>(T node)
        where T : TSqlFragment
        {
            TSqlTokenType lastOperator = TSqlTokenType.None;

            for (int i = node.LastTokenIndex, start = node.FirstTokenIndex - 1; i > start; i--)
            {
                var token = node.ScriptTokenStream[i];
                var oper = token.TokenType;
                if (IsOperator(oper))
                {
                    if (lastOperator == oper)
                    {
                        HandleTokenError(token);
                    }
                    else
                    {
                        lastOperator = oper;
                    }
                }
                else if (!ScriptDomExtension.IsSkippableTokens(oper))
                {
                    lastOperator = TSqlTokenType.None;
                }
            }
        }
    }
}
