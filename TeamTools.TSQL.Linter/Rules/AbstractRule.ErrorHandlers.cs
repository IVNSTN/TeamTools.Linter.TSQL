using Microsoft.SqlServer.TransactSql.ScriptDom;
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
        public void HandleLineError(int line, int pos, string details = default) => ReportRuleViolation(line, pos, details);

        public void HandleFileError(string details = default) => ReportRuleViolation(0, 1, details);

        protected void HandleNodeError(TSqlFragment fragment) => HandleNodeError(fragment, default);

        protected void HandleNodeError(TSqlFragment fragment, string details)
            => ReportRuleViolation(fragment.StartLine, fragment.StartColumn, details, fragment);

        protected void HandleNodeErrorIfAny(IEnumerable<TSqlFragment> node)
            => HandleNodeErrorIfAny(node?.FirstOrDefault());

        protected void HandleNodeErrorIfAny(TSqlFragment node)
            => HandleNodeErrorIfAny(node, default); // FIXME : get rid of this crutch

        protected void HandleTokenError(TSqlParserToken token, string details = default)
            => ReportRuleViolation(token.Line, token.Column, details, null, token.Text.Length);

        protected void HandleNodeErrorIfAny(TSqlFragment node, string details)
        {
            if (node != null)
            {
                HandleNodeError(node, details);
            }
        }
    }
}
