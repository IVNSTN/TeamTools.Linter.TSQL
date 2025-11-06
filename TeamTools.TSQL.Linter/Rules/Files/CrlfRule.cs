using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0303", "CRLF")]
    internal sealed class CrlfRule : AbstractRule
    {
        private readonly Regex pattern = new Regex(@"[^\r]+\n", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public CrlfRule() : base()
        {
        }

        public Regex Pattern => pattern;

        public override void Visit(TSqlScript script)
        {
            int n = script.ScriptTokenStream.Count;
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(script.ScriptTokenStream[i].Text))
                {
                    continue;
                }

                CheckLineEndings(script.ScriptTokenStream[i].Text, script.ScriptTokenStream[i].Line);
            }
        }

        public void CheckLineEndings(string scriptText, int line)
        {
            if (string.IsNullOrEmpty(scriptText))
            {
                return;
            }

            int newLineCount = scriptText.DelimiterCount('\n');
            int carriageCount = scriptText.DelimiterCount('\r');

            if (newLineCount == 0 && carriageCount == 0)
            {
                return;
            }

            if (newLineCount != carriageCount)
            {
                HandleLineError(line, 0);
            }

            if (!pattern.IsMatch(scriptText))
            {
                return;
            }

            HandleLineError(line, 0);
        }
    }
}
