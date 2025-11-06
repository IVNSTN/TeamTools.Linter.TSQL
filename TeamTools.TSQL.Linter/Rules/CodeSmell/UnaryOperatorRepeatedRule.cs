using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0250", "REDUNDANT_OPERATOR")]
    internal sealed class UnaryOperatorRepeatedRule : AbstractRule
    {
        private readonly IList<TSqlTokenType> operators = new List<TSqlTokenType>();

        public UnaryOperatorRepeatedRule() : base()
        {
            operators.Add(TSqlTokenType.Plus);
            operators.Add(TSqlTokenType.Minus);
            operators.Add(TSqlTokenType.EqualsSign);
            operators.Add(TSqlTokenType.Star);
            operators.Add(TSqlTokenType.Not);
            operators.Add(TSqlTokenType.And);
            operators.Add(TSqlTokenType.Or);
            operators.Add(TSqlTokenType.Tilde);
            operators.Add(TSqlTokenType.Bang);
            operators.Add(TSqlTokenType.GreaterThan);
            operators.Add(TSqlTokenType.LessThan);
        }

        public override void Visit(BooleanExpression node)
        {
            ValidateOperators(node);
        }

        public override void Visit(ScalarExpression node)
        {
            ValidateOperators(node);
        }

        private void ValidateOperators(TSqlFragment node)
        {
            // some parser bug
            if (node.FirstTokenIndex < 0)
            {
                return;
            }

            TSqlTokenType lastOperator = TSqlTokenType.None;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                switch (node.ScriptTokenStream[i].TokenType)
                {
                    case TSqlTokenType.WhiteSpace:
                    case TSqlTokenType.MultilineComment:
                    case TSqlTokenType.SingleLineComment:
                        // ignoring
                        break;
                    case TSqlTokenType oper when operators.Contains(oper):
                        {
                            if (lastOperator == oper)
                            {
                                HandleTokenError(node.ScriptTokenStream[i]);
                            }
                            else
                            {
                                lastOperator = oper;
                            }

                            break;
                        }

                    default:
                        lastOperator = TSqlTokenType.None;
                        break;
                }
            }
        }
    }
}
