using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Multi-word instruction parts should be separated by single whitespace only.
    /// </summary>
    [RuleIdentity("FM0276", "TWO_WORD_WITH_SINGLE_SPACE")]
    internal sealed partial class SingleSpaceInTwoWordInstructionRule : AbstractRule
    {
        public SingleSpaceInTwoWordInstructionRule() : base()
        {
        }

        private static bool IsValidFormatting(TSqlFragment node, int firstToken, int lastToken, out int violationPosition)
        {
            Debug.Assert(lastToken > firstToken, $"broken token range: {firstToken}<{lastToken} for {node.GetType().Name}");

            violationPosition = default;

            if (lastToken <= firstToken)
            {
                // unable to validate
                return true;
            }

            // fragment can include comments before and after, linebreaks and so on
            // which are not a part of validated expression
            while (lastToken > firstToken && ScriptDomExtension.IsNonStatementToken(node.ScriptTokenStream[lastToken].TokenType))
            {
                lastToken--;
            }

            while (lastToken > firstToken && ScriptDomExtension.IsNonStatementToken(node.ScriptTokenStream[firstToken].TokenType))
            {
                firstToken++;
            }

            if (lastToken <= firstToken)
            {
                // unable to validate
                return true;
            }

            lastToken++;
            for (int i = firstToken; i < lastToken; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.WhiteSpace && token.Text.Length > 1)
                {
                    violationPosition = i;
                    return false;
                }
            }

            return true;
        }

        private void ValidateSpaceBetween(TSqlFragment node, int firstToken, int lastToken, string instructionInfo = default)
        {
            if (!IsValidFormatting(node, firstToken, lastToken, out int violationPosition))
            {
                HandleTokenError(node.ScriptTokenStream[violationPosition], instructionInfo);
            }
        }
    }
}
