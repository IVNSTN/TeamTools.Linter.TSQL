using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0230", "WHITESPACE_AROUND_OPERATOR")]
    internal sealed class WhitespaceAroundOperatorRule : AbstractRule
    {
        private static readonly HashSet<TSqlTokenType> Operators;

        static WhitespaceAroundOperatorRule()
        {
            Operators = new HashSet<TSqlTokenType>
            {
                TSqlTokenType.Plus,
                TSqlTokenType.Minus,
                TSqlTokenType.Percent,
                TSqlTokenType.EqualsSign,
                TSqlTokenType.Divide,
                TSqlTokenType.Star,
                TSqlTokenType.Not,
                TSqlTokenType.And,
                TSqlTokenType.Or,
                TSqlTokenType.Ampersand,
                TSqlTokenType.Tilde,
                TSqlTokenType.Bang,
                TSqlTokenType.BitwiseAndEquals,
                TSqlTokenType.BitwiseOrEquals,
                TSqlTokenType.BitwiseXorEquals,
                TSqlTokenType.AddEquals,
                TSqlTokenType.SubtractEquals,
                TSqlTokenType.MultiplyEquals,
                TSqlTokenType.DivideEquals,
                TSqlTokenType.ModEquals,
                TSqlTokenType.GreaterThan,
                TSqlTokenType.LessThan,
            };
        }

        public WhitespaceAroundOperatorRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var whiteSpaceCounter = new WhiteSpaceCounterVisitor(Operators, ViolationHandlerPerLine);
            node.Accept(whiteSpaceCounter);
        }

        private sealed class WhiteSpaceCounterVisitor : TSqlFragmentVisitor
        {
            private readonly Action<int, int, string> callback;
            private readonly HashSet<TSqlTokenType> operators;

            private int lastVisitedToken = -1;

            public WhiteSpaceCounterVisitor(
                HashSet<TSqlTokenType> operators,
                Action<int, int, string> callback)
            {
                this.operators = operators;
                this.callback = callback;
            }

            public int TotalErrorcount { get; private set; } = 0;

            public override void Visit(ScalarExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(BooleanExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(DeclareVariableElement node) => DetectExpressionWhitespaceErrors(node, node.Value != null);

            public override void Visit(SetVariableStatement node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(SelectSetVariable node) => DetectExpressionWhitespaceErrors(node);

            private static bool IsCombinableOperator(TSqlTokenType tokenType)
            {
                return tokenType == TSqlTokenType.GreaterThan
                    || tokenType == TSqlTokenType.LessThan
                    || tokenType == TSqlTokenType.EqualsSign
                    || tokenType == TSqlTokenType.Bang
                    || tokenType == TSqlTokenType.EqualsSign;
            }

            private static bool IsExpressionBreaker(TSqlTokenType tokenType)
            {
                return tokenType == TSqlTokenType.Comma
                    || tokenType == TSqlTokenType.LeftParenthesis
                    || tokenType == TSqlTokenType.If
                    || tokenType == TSqlTokenType.When
                    || tokenType == TSqlTokenType.Then
                    || tokenType == TSqlTokenType.Else
                    || tokenType == TSqlTokenType.While
                    || tokenType == TSqlTokenType.EqualsSign;
            }

            private static bool IsLeadingOperator(TSqlTokenType tokenType)
            {
                return tokenType == TSqlTokenType.Minus
                    || tokenType == TSqlTokenType.Plus
                    || tokenType == TSqlTokenType.Not
                    || tokenType == TSqlTokenType.Tilde;
            }

            // TODO : refactoring needed
            private void DetectExpressionWhitespaceErrors<T>(T node, bool firstEqualsSignMayHaveMultipleLeadingSpaces = false)
            where T : TSqlFragment
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                if (node.LastTokenIndex <= lastVisitedToken)
                {
                    // kilroy waz here
                    return;
                }

                lastVisitedToken = node.LastTokenIndex;

                int whiteSpaceCount = 0;
                int lastOtherSymbolLine = -1;
                int lastOtherSymbolTokenIndex = -1;
                TSqlParserToken lastOtherSymbolToken = default;
                TSqlParserToken lastOperatorToken = default;
                int lastOperatorTokenIndex = -1;
                int lastOperatorErrorCount = 0;
                int start = node.FirstTokenIndex;
                int end = node.LastTokenIndex + 1;

                for (int i = start; i < end; i++)
                {
                    var token = node.ScriptTokenStream[i];

                    if (token.TokenType == TSqlTokenType.WhiteSpace)
                    {
                        // counting spaces
                        whiteSpaceCount += token.Text.Replace(Environment.NewLine, "").Length;
                    }
                    else if ((lastOperatorToken != null)
                        && IsCombinableOperator(token.TokenType)
                        && IsCombinableOperator(lastOperatorToken.TokenType))
                    {
                        // <>, >= and so one are a single combined operator
                        // so when located second part just switching token index
                        lastOperatorToken = token;
                        lastOperatorTokenIndex = i;
                    }
                    else if (operators.Contains(token.TokenType))
                    {
                        if (lastOperatorErrorCount > 0)
                        {
                            ReportTokenRuleViolation(node, lastOperatorTokenIndex, i - 1);
                        }

                        if (lastOperatorTokenIndex > lastOtherSymbolTokenIndex)
                        {
                            lastOtherSymbolTokenIndex = -1;
                            lastOtherSymbolToken = null;
                        }

                        lastOperatorToken = token;
                        lastOperatorTokenIndex = i;
                        lastOperatorErrorCount = 0;

                        if (lastOtherSymbolTokenIndex > -1)
                        {
                            // checking spaces before operator
                            if (IsLeadingOperator(lastOperatorToken.TokenType)
                            && (lastOtherSymbolToken.TokenType == TSqlTokenType.LeftParenthesis))
                            {
                                // leading minus inside parenthesis without space before is ok
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (lastOperatorToken.TokenType == TSqlTokenType.Star
                            && (lastOtherSymbolToken.TokenType == TSqlTokenType.LeftParenthesis))
                            {
                                // Count(*) and so on do not need spaces
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if ((whiteSpaceCount < 1) && (token.Column > 0))
                            {
                                lastOperatorErrorCount++;
                            }
                            else if (firstEqualsSignMayHaveMultipleLeadingSpaces && whiteSpaceCount >= 1
                                && lastOperatorToken.TokenType == TSqlTokenType.EqualsSign)
                            {
                                // formatting = as table is fine for declare
                                firstEqualsSignMayHaveMultipleLeadingSpaces = false;
                            }
                            else if ((whiteSpaceCount > 1) && (lastOtherSymbolLine == token.Line))
                            {
                                // if there was a line break then many spaces before'd be expected
                                lastOperatorErrorCount++;
                            }
                        }

                        whiteSpaceCount = 0;
                    }
                    else
                    {
                        if (lastOperatorTokenIndex > -1)
                        {
                            // checking spaces after operator
                            if ((lastOperatorToken.TokenType == TSqlTokenType.Minus || lastOperatorToken.TokenType == TSqlTokenType.Plus)
                            && (lastOtherSymbolTokenIndex == -1 || IsExpressionBreaker(lastOtherSymbolToken.TokenType))
                            // - () space required
                            && (token.TokenType != TSqlTokenType.LeftParenthesis))
                            {
                                // leading minus/plus without space after is ok
                                // except before (
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (lastOperatorToken.TokenType == TSqlTokenType.Star
                            && (token.TokenType == TSqlTokenType.RightParenthesis)
                            && (lastOtherSymbolTokenIndex != -1 && (lastOtherSymbolToken.TokenType == TSqlTokenType.LeftParenthesis)))
                            {
                                // Count(*) and so on do not need spaces
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (token.Line > lastOperatorToken.Line)
                            {
                                // after linebreak there can be many spaces or none
                            }
                            else if (whiteSpaceCount != 1)
                            {
                                lastOperatorErrorCount++;
                            }

                            if (lastOperatorErrorCount > 0)
                            {
                                ReportTokenRuleViolation(node, lastOperatorTokenIndex, i - 1);
                            }
                        }

                        // reset if something else met
                        whiteSpaceCount = 0;
                        lastOperatorErrorCount = 0;
                        lastOperatorToken = null;
                        lastOperatorTokenIndex = -1;
                        lastOtherSymbolTokenIndex = i;
                        lastOtherSymbolToken = token;
                        lastOtherSymbolLine = token.Line;
                    }
                }
            }

            private void ReportTokenRuleViolation(TSqlFragment node, int tokenIndex, int rangeEndTokenIndex)
            {
                var token = node.ScriptTokenStream[tokenIndex];
                callback(token.Line, token.Column, default);
            }
        }
    }
}
