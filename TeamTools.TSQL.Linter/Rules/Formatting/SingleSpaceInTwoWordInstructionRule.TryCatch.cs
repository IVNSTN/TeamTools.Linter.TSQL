using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Validating space between words in TRY-CATCH.
    /// </summary>
    internal partial class SingleSpaceInTwoWordInstructionRule
    {
        public override void Visit(TryCatchStatement node)
        {
            // begin try
            if (node.LookForwardFor(TSqlTokenType.Begin, out int beginTryStart)
            && node.LookForwardFor(beginTryStart + 1, node.LastTokenIndex, "TRY", out int beginTryEnd))
            {
                ValidateSpaceBetween(node, beginTryStart, beginTryEnd, "BEGIN TRY");
            }
            else
            {
                return;
            }

            // end try
            int catchStart = node.CatchStatements?.FirstTokenIndex ?? node.LastTokenIndex;
            if (catchStart == -1)
            {
                catchStart = node.LastTokenIndex;
            }

            if (node.LookBackwardFor(catchStart, node.TryStatements.LastTokenIndex, "TRY", out int endTryEnd)
            && node.LookBackwardFor(endTryEnd - 1, node.TryStatements.LastTokenIndex, TSqlTokenType.End, out int endTryStart))
            {
                ValidateSpaceBetween(node, endTryStart, endTryEnd, "END TRY");
            }
            else
            {
                return;
            }

            // begin catch
            if (node.LookForwardFor(endTryEnd + 1, node.LastTokenIndex, TSqlTokenType.Begin, out int beginCatchStart)
            && node.LookForwardFor(beginCatchStart + 1, node.LastTokenIndex, "CATCH", out int beginCatchEnd))
            {
                ValidateSpaceBetween(node, beginCatchStart, beginCatchEnd, "BEGIN CATCH");
            }
            else
            {
                return;
            }

            // end cath
            if (node.LookBackwardFor(node.LastTokenIndex, beginCatchEnd + 1, "CATCH", out int endCatchEnd)
            && node.LookBackwardFor(endCatchEnd - 1, node.FirstTokenIndex, TSqlTokenType.End, out int endCatchStart))
            {
                ValidateSpaceBetween(node, endCatchStart, endCatchEnd, "END CATCH");
            }
        }
    }
}
