using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0309", "TAB_SYMBOL")]
    internal sealed class TabSymbolRule : AbstractRule
    {
        private readonly Regex tabPresence = new Regex(@"\t+", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private readonly int maxViolationsPerFile = 3;

        public TabSymbolRule() : base()
        {
        }

        public override void Visit(TSqlScript script)
        {
            DetectTabs(new StringReader(script.GetFragmentText()), script.ScriptTokenStream[script.FirstTokenIndex].Line);
        }

        private void DetectTabs(TextReader reader, int startLine)
        {
            string line;
            int lineNumber = startLine - 1;
            int violationCount = 0;

            while ((violationCount < maxViolationsPerFile) && (line = reader.ReadLine()) != null)
            {
                lineNumber++;

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var tabs = tabPresence.Match(line);

                if (tabs.Success)
                {
                    // +1 because index is zero-based
                    ReportRuleViolation(lineNumber, tabs.Index + 1, default, default, tabs.Value.Length);
                    violationCount++;
                }
            }
        }
    }
}
