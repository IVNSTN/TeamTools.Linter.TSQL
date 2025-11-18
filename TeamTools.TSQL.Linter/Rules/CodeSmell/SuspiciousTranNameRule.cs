using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0990", "SUSPICIOUS_TRAN_NAME")]
    internal sealed class SuspiciousTranNameRule : AbstractRule
    {
        private static readonly HashSet<string> SuspiciousNames;

        private static readonly HashSet<TSqlTokenType> SuspiciousTokens = new HashSet<TSqlTokenType>
        {
            TSqlTokenType.Return,
            TSqlTokenType.Exec,
            TSqlTokenType.Execute,
        };

        private static readonly HashSet<TSqlTokenType> CommandTerminators = new HashSet<TSqlTokenType>
        {
            TSqlTokenType.Semicolon,
            TSqlTokenType.Go,
        };

        static SuspiciousTranNameRule()
        {
            // TODO : load (amost) all known keywords form metadata?
            SuspiciousNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "THROW",
                "RAISERROR",
                "RETURN",
                "BREAK",
                "CONTINUE",
                "COMMIT",
                "ROLLBACK",
                "TRAN",
                "TRANSACTION",
                "EXEC",
                "EXECUTE",
                "BEGIN",
                "END",
                "SET",
                "SELECT",
                "DECLARE",
                "DBCC",
                "PRINT",
                "RECEIVE",
                "SEND",
            };
        }

        public SuspiciousTranNameRule() : base()
        {
        }

        public override void Visit(TransactionStatement node)
        {
            if (string.IsNullOrEmpty(node.Name?.Value))
            {
                // no name provided
                if (IsNextTokenSuspicious(node, out string badWord))
                {
                    // but the following code pretends to be tran name
                    HandleNodeError(node, badWord);
                }

                return;
            }

            if (!SuspiciousNames.Contains(node.Name.Value))
            {
                // name is good
                return;
            }

            HandleNodeError(node.Name, node.Name.Value);
        }

        private static bool IsNextTokenSuspicious(TSqlFragment node, out string badWord)
        {
            badWord = "";
            int startPos = node.LastTokenIndex;
            var start = node.ScriptTokenStream[startPos];

            if (CommandTerminators.Contains(start.TokenType))
            {
                // doing nothing if last token was ;
                return false;
            }

            bool violationFound = false;
            int startLine = start.Line;
            int currentPos = startPos + 1;
            int tokenCount = node.ScriptTokenStream.Count;

            // considering linebreak means everyone understands
            // that next token/keyword is a separate command
            while (currentPos < tokenCount
            && startLine == node.ScriptTokenStream[currentPos].Line
            && !violationFound)
            {
                var currentToken = node.ScriptTokenStream[currentPos];

                if (CommandTerminators.Contains(currentToken.TokenType))
                {
                    break;
                }

                violationFound = SuspiciousTokens.Contains(currentToken.TokenType)
                    || (!string.IsNullOrWhiteSpace(currentToken.Text)
                    && SuspiciousNames.Contains(currentToken.Text));

                if (violationFound)
                {
                    badWord = currentToken.Text;
                }

                currentPos++;
            }

            return violationFound;
        }
    }
}
