using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0230", "WHITESPACE_AROUND_OPERATOR")]
    internal sealed class WhitespaceAroundOperatorRule : AbstractRule
    {
        private readonly IList<TSqlTokenType> operators = new List<TSqlTokenType>();
        private readonly IList<TSqlTokenType> combinableOperators = new List<TSqlTokenType>();
        private readonly IList<TSqlTokenType> expressionBreakers = new List<TSqlTokenType>();
        private readonly IList<TSqlTokenType> leadingOperators = new List<TSqlTokenType>();

        public WhitespaceAroundOperatorRule() : base()
        {
            operators.Add(TSqlTokenType.Plus);
            operators.Add(TSqlTokenType.Minus);
            operators.Add(TSqlTokenType.Percent);
            operators.Add(TSqlTokenType.EqualsSign);
            operators.Add(TSqlTokenType.Divide);
            operators.Add(TSqlTokenType.Star);
            operators.Add(TSqlTokenType.Not);
            operators.Add(TSqlTokenType.And);
            operators.Add(TSqlTokenType.Or);
            operators.Add(TSqlTokenType.Ampersand);
            operators.Add(TSqlTokenType.Tilde);
            operators.Add(TSqlTokenType.Bang);
            operators.Add(TSqlTokenType.BitwiseAndEquals);
            operators.Add(TSqlTokenType.BitwiseOrEquals);
            operators.Add(TSqlTokenType.BitwiseXorEquals);
            operators.Add(TSqlTokenType.AddEquals);
            operators.Add(TSqlTokenType.SubtractEquals);
            operators.Add(TSqlTokenType.MultiplyEquals);
            operators.Add(TSqlTokenType.DivideEquals);
            operators.Add(TSqlTokenType.ModEquals);
            operators.Add(TSqlTokenType.GreaterThan);
            operators.Add(TSqlTokenType.LessThan);

            combinableOperators.Add(TSqlTokenType.GreaterThan);
            combinableOperators.Add(TSqlTokenType.LessThan);
            combinableOperators.Add(TSqlTokenType.EqualsSign);
            combinableOperators.Add(TSqlTokenType.Bang);

            leadingOperators.Add(TSqlTokenType.Minus);
            leadingOperators.Add(TSqlTokenType.Plus);
            leadingOperators.Add(TSqlTokenType.Not);
            leadingOperators.Add(TSqlTokenType.Tilde);

            expressionBreakers.Add(TSqlTokenType.Comma);
            expressionBreakers.Add(TSqlTokenType.LeftParenthesis);
            expressionBreakers.Add(TSqlTokenType.If);
            expressionBreakers.Add(TSqlTokenType.When);
            expressionBreakers.Add(TSqlTokenType.Then);
            expressionBreakers.Add(TSqlTokenType.Else);
            expressionBreakers.Add(TSqlTokenType.While);
            expressionBreakers.Add(TSqlTokenType.EqualsSign);
        }

        public override void Visit(TSqlBatch node)
        {
            combinableOperators.Add(TSqlTokenType.EqualsSign);
            var whiteSpaceCounter = new WhiteSpaceCounterVisitor(operators, combinableOperators, leadingOperators, expressionBreakers);
            node.Accept(whiteSpaceCounter);

            if (whiteSpaceCounter.TotalErrorcount == 0)
            {
                return;
            }

            foreach (var err in whiteSpaceCounter.ReportedTokenRanges)
            {
                HandleTokenError(node.ScriptTokenStream[err.Key]);
            }
        }

        private class WhiteSpaceCounterVisitor : TSqlFragmentVisitor
        {
            private readonly IList<TSqlTokenType> operators;
            private readonly IList<TSqlTokenType> combinableOperators;
            private readonly IList<TSqlTokenType> leadingOperators;
            private readonly IList<TSqlTokenType> expressionBreakers;
            private readonly IList<KeyValuePair<int, int>> reportedTokenRanges = new List<KeyValuePair<int, int>>();

            public WhiteSpaceCounterVisitor(
                IList<TSqlTokenType> operators,
                IList<TSqlTokenType> combinableOperators,
                IList<TSqlTokenType> leadingOperators,
                IList<TSqlTokenType> expressionBreakers)
            {
                this.operators = operators;
                this.combinableOperators = combinableOperators;
                this.expressionBreakers = expressionBreakers;
                this.leadingOperators = leadingOperators;
            }

            public IList<KeyValuePair<int, int>> ReportedTokenRanges => reportedTokenRanges;

            public int TotalErrorcount { get; private set; } = 0;

            public override void Visit(ScalarExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(UnaryExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(BinaryExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(BooleanExpression node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(DeclareVariableElement node) => DetectExpressionWhitespaceErrors(node, node.Value != null);

            public override void Visit(SetVariableStatement node) => DetectExpressionWhitespaceErrors(node);

            public override void Visit(SelectSetVariable node) => DetectExpressionWhitespaceErrors(node);

            protected void DetectExpressionWhitespaceErrors(TSqlFragment node, bool firstEqualsSignMayHaveMultipleLeadingSpaces = false)
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                int whiteSpaceCount = 0;
                int lastOtherSymbolLine = -1;
                int lastOtherSymbolTokenIndex = -1;
                int lastOperatorTokenIndex = -1;
                int lastOperatorErrorCount = 0;
                int start = node.FirstTokenIndex;
                int end = node.LastTokenIndex;

                for (int i = start; i <= end; i++)
                {
                    if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                    {
                        // counting spaces
                        whiteSpaceCount += node.ScriptTokenStream[i].Text.Replace(Environment.NewLine, "").Length;
                    }
                    else if ((lastOperatorTokenIndex >= 0)
                        && combinableOperators.Contains(node.ScriptTokenStream[i].TokenType)
                        && combinableOperators.Contains(node.ScriptTokenStream[lastOperatorTokenIndex].TokenType))
                    {
                        // <>, >= and so one are a single combined operator
                        // so when located second part just switching token index
                        lastOperatorTokenIndex = i;
                    }
                    else if (operators.Contains(node.ScriptTokenStream[i].TokenType))
                    {
                        if (lastOperatorErrorCount > 0)
                        {
                            ReportTokenRuleViolation(node, lastOperatorTokenIndex, i - 1);
                        }

                        if (lastOperatorTokenIndex > lastOtherSymbolTokenIndex)
                        {
                            lastOtherSymbolTokenIndex = -1;
                        }

                        lastOperatorTokenIndex = i;
                        lastOperatorErrorCount = 0;

                        if (lastOtherSymbolTokenIndex > -1)
                        {
                            // checking spaces before operator
                            if (leadingOperators.Contains(node.ScriptTokenStream[lastOperatorTokenIndex].TokenType)
                            && (node.ScriptTokenStream[lastOtherSymbolTokenIndex].TokenType == TSqlTokenType.LeftParenthesis))
                            {
                                // leading minus inside parenthesis without space before is ok
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (node.ScriptTokenStream[lastOperatorTokenIndex].TokenType == TSqlTokenType.Star
                            && (node.ScriptTokenStream[lastOtherSymbolTokenIndex].TokenType == TSqlTokenType.LeftParenthesis))
                            {
                                // Count(*) and so on do not need spaces
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if ((whiteSpaceCount < 1) && (node.ScriptTokenStream[i].Column > 0))
                            {
                                lastOperatorErrorCount++;
                            }
                            else if (firstEqualsSignMayHaveMultipleLeadingSpaces && whiteSpaceCount >= 1
                                && node.ScriptTokenStream[lastOperatorTokenIndex].TokenType == TSqlTokenType.EqualsSign)
                            {
                                // formatting = as table is fine for declare
                                firstEqualsSignMayHaveMultipleLeadingSpaces = false;
                            }
                            else if ((whiteSpaceCount > 1) && (lastOtherSymbolLine == node.ScriptTokenStream[i].Line))
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
                            if ((node.ScriptTokenStream[lastOperatorTokenIndex].TokenType == TSqlTokenType.Minus || node.ScriptTokenStream[lastOperatorTokenIndex].TokenType == TSqlTokenType.Plus)
                            && (lastOtherSymbolTokenIndex == -1 || expressionBreakers.Contains(node.ScriptTokenStream[lastOtherSymbolTokenIndex].TokenType))
                            // - () space required
                            && (node.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis))
                            {
                                // leading minus/plus without space after is ok
                                // except before (
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (node.ScriptTokenStream[lastOperatorTokenIndex].TokenType == TSqlTokenType.Star
                            && (node.ScriptTokenStream[i].TokenType == TSqlTokenType.RightParenthesis)
                            && (lastOtherSymbolTokenIndex != -1 && (node.ScriptTokenStream[lastOtherSymbolTokenIndex].TokenType == TSqlTokenType.LeftParenthesis)))
                            {
                                // Count(*) and so on do not need spaces
                                if (whiteSpaceCount > 0)
                                {
                                    lastOperatorErrorCount++;
                                }
                            }
                            else if (node.ScriptTokenStream[i].Line > node.ScriptTokenStream[lastOperatorTokenIndex].Line)
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
                        lastOperatorTokenIndex = -1;
                        lastOtherSymbolTokenIndex = i;
                        lastOtherSymbolLine = node.ScriptTokenStream[i].Line;
                    }
                }
            }

            private void ReportTokenRuleViolation(TSqlFragment node, int tokenIndex, int rangeEndTokenIndex)
            {
                TotalErrorcount++;

                // if given token is within any of already reported tokens - doing nothing
                foreach (var range in ReportedTokenRanges)
                {
                    if (range.Key <= tokenIndex && tokenIndex <= range.Value)
                    {
                        return;
                    }
                }

                ReportedTokenRanges.Add(new KeyValuePair<int, int>(tokenIndex, rangeEndTokenIndex < tokenIndex ? tokenIndex : rangeEndTokenIndex));
            }
        }
    }
}
