using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class ParenthesisParser
    {
        private readonly TSqlFragment node;

        private readonly IDictionary<int, ParenthesisInfo> openParenthesises = new Dictionary<int, ParenthesisInfo>(1000);

        public ParenthesisParser(TSqlFragment node)
        {
            this.node = node;
        }

        public IDictionary<int, ParenthesisInfo> OpenParenthesises => openParenthesises;

        public void Parse()
        {
            int currentParenthesisToken = -1;
            int nestLevel = 0;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            var subqueries = new SubqueryVisitor();
            node.Accept(subqueries);

            for (int i = start; i <= end; i++)
            {
                switch (node.ScriptTokenStream[i].TokenType)
                {
                    case TSqlTokenType.LeftParenthesis:
                        {
                            // self and parent link
                            OpenParenthesises.Add(i, new ParenthesisInfo(i, currentParenthesisToken));
                            if (currentParenthesisToken > -1)
                            {
                                OpenParenthesises[currentParenthesisToken].HasNestedParenthesis = true;
                                if (subqueries.SubqueryOpenParenthesis.Contains(i))
                                {
                                    // scalar subquery needs parenthesis
                                    OpenParenthesises[currentParenthesisToken].FirstMeaningfullTokenIndex = i;
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
                            if (OpenParenthesises.ContainsKey(currentParenthesisToken))
                            {
                                OpenParenthesises[currentParenthesisToken].CloseTokenIndex = i;
                                if (-1 == OpenParenthesises[currentParenthesisToken].LastMeaningfullTokenIndex)
                                {
                                    OpenParenthesises[currentParenthesisToken].LastMeaningfullTokenIndex = OpenParenthesises[currentParenthesisToken].FirstMeaningfullTokenIndex;
                                }

                                currentParenthesisToken = OpenParenthesises[currentParenthesisToken].ParentTokenIndex;
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
                                if (-1 == OpenParenthesises[currentParenthesisToken].FirstMeaningfullTokenIndex)
                                {
                                    OpenParenthesises[currentParenthesisToken].FirstMeaningfullTokenIndex = i;
                                }
                                else
                                {
                                    OpenParenthesises[currentParenthesisToken].LastMeaningfullTokenIndex = i;
                                }
                            }

                            break;
                        }
                }
            }
        }

        private class SubqueryVisitor : TSqlFragmentVisitor
        {
            public List<int> SubqueryOpenParenthesis { get; } = new List<int>();

            public override void Visit(ScalarSubquery node)
            {
                int i = node.FirstTokenIndex;

                while (i < node.LastTokenIndex)
                {
                    if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis)
                    {
                        SubqueryOpenParenthesis.Add(i);
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }
}
