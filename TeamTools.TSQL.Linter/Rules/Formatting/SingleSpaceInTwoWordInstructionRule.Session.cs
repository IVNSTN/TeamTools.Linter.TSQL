using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Validating space between words in BEGIN TRAN, EXECUTE AS etc.
    /// </summary>
    internal partial class SingleSpaceInTwoWordInstructionRule
    {
        private static readonly HashSet<string> ExecuteAsOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OWNER",
            "CALLER",
            "SELF",
        };

        // BEGIN/COMMIT/ROLLBACK/SAVE TRANSACTION
        public override void Visit(TransactionStatement node)
        {
            if (node.FirstTokenIndex == node.LastTokenIndex)
            {
                // single word COMMIT or ROLLBACK
                return;
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, ((TSqlFragment)node.Name ?? node).LastTokenIndex, "TRANSACTION");
        }

        // WITH EXECUTE AS option
        public override void Visit(ExecuteAsClause node)
        {
            // TODO : there is a bug in ScriptDom. remove this search after bugfix;
            int lastIndex = node.LastTokenIndex;
            if (node.Literal != null)
            {
                lastIndex--;
            }
            else if (node.LastTokenIndex == node.FirstTokenIndex)
            {
                int n = node.ScriptTokenStream.Count;

                while (lastIndex < n
                && !(node.ScriptTokenStream[lastIndex].TokenType == TSqlTokenType.Identifier
                    && ExecuteAsOptions.Contains(node.ScriptTokenStream[lastIndex].Text)))
                {
                    lastIndex++;
                }
            }

            if (lastIndex >= node.ScriptTokenStream.Count)
            {
                // failed to find anything appropriate
                return;
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, lastIndex, "EXECUTE AS");
        }

        // EXECUTE AS statement
        public override void Visit(ExecuteAsStatement node)
        {
            int lastToken = node.ExecuteContext.FirstTokenIndex;
            // If there was no Principal, then the last word is OWNER/CALLER and so on - we need it.
            if (node.ExecuteContext.Principal != null)
            {
                // If principal was mentioned, then moving to the left of " = 'usr' "
                while (lastToken > node.FirstTokenIndex && node.ScriptTokenStream[lastToken].TokenType != TSqlTokenType.EqualsSign)
                {
                    lastToken--;
                }

                if (node.ScriptTokenStream[lastToken].TokenType == TSqlTokenType.EqualsSign)
                {
                    lastToken--;
                }
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, lastToken, "EXECUTE AS");
        }

        // SET CONTEXT_INFO, DATEFIRST, etc.
        public override void Visit(GeneralSetCommand node)
        {
            // TODO : There is a bug in ScriptDom. Remove this loop after bug fix.
            int firstToken = node.FirstTokenIndex;
            while (firstToken > 0 && node.ScriptTokenStream[firstToken].TokenType != TSqlTokenType.Set)
            {
                firstToken--;
            }

            ValidateSpaceBetween(node, firstToken, node.Parameter.FirstTokenIndex, "SET <option>");
        }
    }
}
