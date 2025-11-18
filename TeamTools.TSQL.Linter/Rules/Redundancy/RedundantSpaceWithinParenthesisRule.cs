using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0235", "WHITESPACE_IN_PARENTHESIS")]
    internal sealed class RedundantSpaceWithinParenthesisRule : ScriptAnalysisServiceConsumingRule
    {
        public RedundantSpaceWithinParenthesisRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var parenthesis = GetService<ParenthesisParser>(node);

            int badTokenIndex;

            foreach (var i in parenthesis.OpenParenthesises.Values)
            {
                badTokenIndex = -1;

                if (!i.HasMeaningfullToken)
                {
                    // and no line breaks
                    if ((i.CloseTokenIndex > i.OpenTokenIndex + 1) && !i.HasNestedParenthesis)
                    {
                        badTokenIndex = i.CloseTokenIndex;
                    }
                }
                else
                {
                    // if a line break was between tokens then there might be some allowed spaces
                    // open
                    if ((i.FirstMeaningfullTokenIndex > i.OpenTokenIndex + 1)
                        && (node.ScriptTokenStream[i.FirstMeaningfullTokenIndex].Line == node.ScriptTokenStream[i.CloseTokenIndex].Line))
                    {
                        if (IsWhitespace(node, i.OpenTokenIndex, i.FirstMeaningfullTokenIndex))
                        {
                            badTokenIndex = i.OpenTokenIndex;
                        }
                    }

                    // close
                    if ((i.LastMeaningfullTokenIndex < i.CloseTokenIndex - 1)
                        && (node.ScriptTokenStream[i.FirstMeaningfullTokenIndex].Line == node.ScriptTokenStream[i.CloseTokenIndex].Line))
                    {
                        if (IsWhitespace(node, i.LastMeaningfullTokenIndex, i.CloseTokenIndex))
                        {
                            badTokenIndex = i.CloseTokenIndex;
                        }
                    }
                }

                // if nested parenthesises is located in the same line with parent
                // then there are possible whitespaces between them
                if (i.ParentTokenIndex > -1 && badTokenIndex == -1)
                {
                    // open
                    var parentOpenTokenIndex = parenthesis.OpenParenthesises[i.ParentTokenIndex].OpenTokenIndex;
                    if (node.ScriptTokenStream[parentOpenTokenIndex].Line == node.ScriptTokenStream[i.OpenTokenIndex].Line
                    && i.OpenTokenIndex > parentOpenTokenIndex + 1)
                    {
                        if (IsWhitespace(node, parentOpenTokenIndex, i.OpenTokenIndex))
                        {
                            badTokenIndex = i.OpenTokenIndex;
                        }
                    }

                    // close
                    var parentCloseTokenIndex = parenthesis.OpenParenthesises[i.ParentTokenIndex].CloseTokenIndex;
                    if (node.ScriptTokenStream[parentCloseTokenIndex].Line == node.ScriptTokenStream[i.CloseTokenIndex].Line
                    && i.CloseTokenIndex < parentCloseTokenIndex - 1)
                    {
                        if (IsWhitespace(node, i.CloseTokenIndex, parentCloseTokenIndex))
                        {
                            badTokenIndex = i.CloseTokenIndex;
                        }
                    }
                }

                if (badTokenIndex > -1)
                {
                    HandleTokenError(node.ScriptTokenStream[badTokenIndex]);
                }
            }
        }

        private static bool IsWhitespace(TSqlFragment node, int start, int end)
        {
            start++;
            for (int i = start; i < end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType != TSqlTokenType.WhiteSpace)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
