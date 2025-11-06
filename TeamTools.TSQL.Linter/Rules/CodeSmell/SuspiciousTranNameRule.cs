using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0990", "SUSPICIOUS_TRAN_NAME")]
    internal sealed class SuspiciousTranNameRule : AbstractRule
    {
        private static readonly ICollection<string> SuspiciousNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ICollection<TSqlTokenType> SuspiciousTokens = new List<TSqlTokenType>();
        private static readonly ICollection<TSqlTokenType> CommandTerminators = new List<TSqlTokenType>();

        static SuspiciousTranNameRule()
        {
            // TODO : load (amost) all known keywords form metadata?
            SuspiciousNames.Add("THROW");
            SuspiciousNames.Add("RAISERROR");
            SuspiciousNames.Add("RETURN");
            SuspiciousNames.Add("BREAK");
            SuspiciousNames.Add("CONTINUE");
            SuspiciousNames.Add("COMMIT");
            SuspiciousNames.Add("ROLLBACK");
            SuspiciousNames.Add("TRAN");
            SuspiciousNames.Add("TRANSACTION");
            SuspiciousNames.Add("EXEC");
            SuspiciousNames.Add("EXECUTE");
            SuspiciousNames.Add("BEGIN");
            SuspiciousNames.Add("END");
            SuspiciousNames.Add("SET");
            SuspiciousNames.Add("SELECT");
            SuspiciousNames.Add("DECLARE");
            SuspiciousNames.Add("DBCC");
            SuspiciousNames.Add("PRINT");
            SuspiciousNames.Add("RECEIVE");
            SuspiciousNames.Add("SEND");

            SuspiciousTokens.Add(TSqlTokenType.Return);
            SuspiciousTokens.Add(TSqlTokenType.Exec);
            SuspiciousTokens.Add(TSqlTokenType.Execute);

            CommandTerminators.Add(TSqlTokenType.Semicolon);
            CommandTerminators.Add(TSqlTokenType.Go);
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

            if (CommandTerminators.Contains(node.ScriptTokenStream[startPos].TokenType))
            {
                // doing nothing if last token was ;
                return false;
            }

            bool violationFound = false;
            int startLine = node.ScriptTokenStream[startPos].Line;
            int currentPos = startPos + 1;

            // considering linebreak means everyone understands
            // that next token/keyword is a separate command
            while (currentPos < node.ScriptTokenStream.Count
            && startLine == node.ScriptTokenStream[currentPos].Line
            && !violationFound)
            {
                if (CommandTerminators.Contains(node.ScriptTokenStream[currentPos].TokenType))
                {
                    break;
                }

                violationFound = SuspiciousTokens.Contains(node.ScriptTokenStream[currentPos].TokenType)
                    || (!string.IsNullOrWhiteSpace(node.ScriptTokenStream[currentPos].Text)
                    && SuspiciousNames.Contains(node.ScriptTokenStream[currentPos].Text));

                if (violationFound)
                {
                    badWord = node.ScriptTokenStream[currentPos].Text;
                }

                currentPos++;
            }

            return violationFound;
        }
    }
}
