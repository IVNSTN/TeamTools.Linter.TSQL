using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Rule error handling methods.
    /// </summary>
    public partial class AbstractRule : ILinterRule
    {
        private Action<TSqlFragment> violationHandler;
        private Action<TSqlFragment, string> violationHandlerWithMessage;
        private Action<int, int, string> violationHandlerPerLine;

        protected Action<TSqlFragment> ViolationHandler => violationHandler ?? (violationHandler = new Action<TSqlFragment>(HandleNodeError));

        protected Action<TSqlFragment, string> ViolationHandlerWithMessage => violationHandlerWithMessage ?? (violationHandlerWithMessage = new Action<TSqlFragment, string>(HandleNodeError));

        protected Action<int, int, string> ViolationHandlerPerLine => violationHandlerPerLine ?? (violationHandlerPerLine = new Action<int, int, string>(HandleLineError));

        public void HandleLineError(int line, int pos, string details = default) => ReportRuleViolation(line, pos, details);

        public void HandleFileError(string details = default) => ReportRuleViolation(0, 1, details);

        protected void HandleNodeError(TSqlFragment fragment) => HandleNodeError(fragment, default);

        protected void HandleNodeError(TSqlFragment fragment, string details)
            => ReportRuleViolation(fragment.StartLine, fragment.StartColumn, details, fragment);

        protected void HandleNodeErrorIfAny(IEnumerable<TSqlFragment> node)
            => HandleNodeErrorIfAny(node?.FirstOrDefault());

        protected void HandleNodeErrorIfAny(TSqlFragment node)
            => HandleNodeErrorIfAny(node, default);

        protected void HandleTokenError(TSqlParserToken token, string details = default)
        {
            // TODO : get rid of this crutch if possible
            if (token.TokenType == TSqlTokenType.WhiteSpace
            && (token.Text.Length > 0)
            && token.Text[0] == '\r'
            && token.Column > 1)
            {
                // When reporting on linebreak it may break delivering to SonarQube
                // ScriptDom parser says: it is on column 14
                // whereas another parser might say: the text ended at position 13
                // - it does not count linebreak as "one more symbol in current line".
                ReportRuleViolation(token.Line, token.Column - 1, details, null, 1);
            }
            else
            {
                ReportRuleViolation(token.Line, token.Column, details, null, token.Text.Length);
            }
        }

        protected void HandleNodeErrorIfAny(TSqlFragment node, string details)
        {
            if (node != null)
            {
                HandleNodeError(node, details);
            }
        }
    }
}
