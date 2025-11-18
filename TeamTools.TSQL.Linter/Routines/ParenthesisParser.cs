using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class ParenthesisParser
    {
        private readonly TSqlFragment node;

        public ParenthesisParser(TSqlFragment node)
        {
            this.node = node;
        }

        public Dictionary<int, ParenthesisInfo> OpenParenthesises { get; } = new Dictionary<int, ParenthesisInfo>(100);

        public void Parse()
        {
            int currentParenthesisToken = -1;
            int nestLevel = 0;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            var subqueries = new SubqueryVisitor();
            node.Accept(subqueries);

            for (int i = start; i < end; i++)
            {
                switch (node.ScriptTokenStream[i].TokenType)
                {
                    case TSqlTokenType.LeftParenthesis:
                        {
                            // self and parent link
                            OpenParenthesises.Add(i, new ParenthesisInfo(i, currentParenthesisToken));
                            if (currentParenthesisToken > -1)
                            {
                                var cur = OpenParenthesises[currentParenthesisToken];
                                cur.HasNestedParenthesis = true;
                                if (subqueries.SubqueryOpenParenthesis.Contains(i))
                                {
                                    // scalar subquery needs parenthesis
                                    cur.FirstMeaningfullTokenIndex = i;
                                }
                            }

                            currentParenthesisToken = i;
                            nestLevel++;
                            break;
                        }

                    case TSqlTokenType.RightParenthesis:
                        {
                            if (currentParenthesisToken == -1)
                            {
                                break;
                            }

                            // switching to parent
                            if (OpenParenthesises.TryGetValue(currentParenthesisToken, out ParenthesisInfo foundToken))
                            {
                                foundToken.CloseTokenIndex = i;
                                if (foundToken.LastMeaningfullTokenIndex == -1)
                                {
                                    foundToken.LastMeaningfullTokenIndex = foundToken.FirstMeaningfullTokenIndex;
                                }

                                currentParenthesisToken = foundToken.ParentTokenIndex;
                            }
                            else
                            {
                                currentParenthesisToken = -1;
                            }

                            nestLevel--;
                            break;
                        }

                    case TSqlTokenType.WhiteSpace:
                    case TSqlTokenType.MultilineComment:
                    case TSqlTokenType.SingleLineComment:
                        // ignoring them
                        break;
                    case TSqlTokenType.EndOfFile:
                    // case TSqlTokenType.End: - CASE ... END - no, thank you
                    case TSqlTokenType.Semicolon:
                        {
                            // end of statement
                            // if is for debug
                            if (currentParenthesisToken == -1)
                            {
                                break;
                            }

                            nestLevel = 0;
                            currentParenthesisToken = -1;
                            break;
                        }

                    default:
                        {
                            if (currentParenthesisToken >= 0)
                            {
                                var token = OpenParenthesises[currentParenthesisToken];
                                if (token.FirstMeaningfullTokenIndex == -1)
                                {
                                    token.FirstMeaningfullTokenIndex = i;
                                }
                                else
                                {
                                    token.LastMeaningfullTokenIndex = i;
                                }
                            }

                            break;
                        }
                }
            }
        }

        private sealed class SubqueryVisitor : TSqlFragmentVisitor
        {
            public HashSet<int> SubqueryOpenParenthesis { get; } = new HashSet<int>();

            public override void Visit(ScalarSubquery node)
            {
                for (int i = node.FirstTokenIndex, n = node.LastTokenIndex; i < n; i++)
                {
                    if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis)
                    {
                        SubqueryOpenParenthesis.Add(i);
                        return;
                    }
                }
            }
        }
    }
}
