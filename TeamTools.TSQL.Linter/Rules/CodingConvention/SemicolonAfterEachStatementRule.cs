using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0268", "SEMICOLON_AFTER_STATEMENT")]
    internal sealed class SemicolonAfterEachStatementRule : AbstractRule
    {
        public SemicolonAfterEachStatementRule() : base()
        {
        }

        public override void Visit(TSqlStatement node)
        {
            if (node is IndexDefinition)
            {
                // this is an inline thing
                return;
            }

            if (node is LabelStatement)
            {
                // it ends with :
                return;
            }

            if (node is CreateViewStatement)
            {
                // SELECT statement will be reported if ; missing in the end
                // if not returned from here then violation will be reported twice
                return;
            }

            // If there is BEGIN-END in any of statement kind below
            // then BeginEndBlockStatement will appear here
            if (node is ProcedureStatementBody)
            {
                return;
            }

            if (node is TriggerStatementBody)
            {
                return;
            }

            if (node is FunctionStatementBody)
            {
                return;
            }

            if (node is IfStatement)
            {
                return;
            }

            if (node is WhileStatement)
            {
                return;
            }

            ValidateSemicolonPresence(node);
        }

        private void ValidateSemicolonPresence(TSqlFragment node)
        {
            int n = node.LastTokenIndex;
            int start = node.FirstTokenIndex;
            while (n >= start && n >= 0
                && node.ScriptTokenStream[n].TokenType == TSqlTokenType.WhiteSpace)
            {
                n--;
            }

            // ends with ; (owned by this very statement)
            if (n <= 0 || node.ScriptTokenStream[n].TokenType == TSqlTokenType.Semicolon)
            {
                return;
            }

            start = n; // last statement instruction

            n = node.LastTokenIndex + 1;
            int tokenCount = node.ScriptTokenStream.Count;
            TSqlParserToken currentToken = null;
            for (n = node.LastTokenIndex + 1; n < tokenCount; n++)
            {
                currentToken = node.ScriptTokenStream[n];
                if (currentToken.TokenType != TSqlTokenType.WhiteSpace
                && currentToken.TokenType != TSqlTokenType.MultilineComment
                && currentToken.TokenType != TSqlTokenType.SingleLineComment)
                {
                    break;
                }
            }

            // ends with ) - subquery cannot be finished with ;
            // inside WAITFOR for example
            if (n < tokenCount && currentToken.TokenType == TSqlTokenType.RightParenthesis)
            {
                return;
            }

            // ends with ; (of outer statement like VIEW)
            if (n <= tokenCount && currentToken.TokenType == TSqlTokenType.Semicolon)
            {
                return;
            }

            HandleTokenError(node.ScriptTokenStream[start]);
        }
    }
}
