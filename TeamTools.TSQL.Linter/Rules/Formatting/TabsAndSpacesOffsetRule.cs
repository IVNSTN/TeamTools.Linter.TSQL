using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0302", "TABS_SPACES_MIX")]
    internal class TabsAndSpacesOffsetRule : AbstractRule, IFileLevelRule
    {
        private readonly Regex mixedOffset = new Regex(@"^(([ ]+\t+)|(\t+[ ]+))+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex spacesOffset = new Regex(@"^[ ]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex tabsOffset = new Regex(@"^\t+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly int maxViolationsPerFile = 3;

        // TODO : multiline literal support
        public TabsAndSpacesOffsetRule() : base()
        {
        }

        public enum OffsetKind
        {
            /// <summary>
            /// unknown offset style
            /// </summary>
            Unknown,

            /// <summary>
            /// tabs
            /// </summary>
            Tabs,

            /// <summary>
            /// spaces
            /// </summary>
            Spaces,
        }

        public override void Visit(TSqlScript script)
        {
            OffsetKind offset = OffsetKind.Unknown;

            CheckTabsAndSpaces(new StringReader(script.GetFragmentText()), script.ScriptTokenStream[script.FirstTokenIndex].Line, ref offset);
        }

        public void VerifyFile(string filePath, TSqlFragment sqlFragment = null)
        {
            OffsetKind offset = OffsetKind.Unknown;

            // TODO : use IFileSystemWrapper
            using (var reader = new System.IO.StreamReader(filePath))
            {
                CheckTabsAndSpaces(reader, 1, ref offset);
            }
        }

        public void VerifyFile(TextReader reader)
        {
            OffsetKind offset = OffsetKind.Unknown;
            CheckTabsAndSpaces(reader, 1, ref offset);
        }

        protected virtual void CheckTabsAndSpaces(TextReader reader, int startLine, ref OffsetKind offset)
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

                if (!CheckTabsAndSpaces(line, ref offset))
                {
                    violationCount++;
                    HandleLineError(lineNumber, 1);
                }
            }
        }

        private bool CheckTabsAndSpaces(string line, ref OffsetKind offset)
        {
            if (mixedOffset.IsMatch(line))
            {
                return false;
            }
            else if (offset == OffsetKind.Unknown)
            {
                if (spacesOffset.IsMatch(line))
                {
                    offset = OffsetKind.Spaces;
                }
                else if (tabsOffset.IsMatch(line))
                {
                    offset = OffsetKind.Tabs;
                }

                return true;
            }
            else
            {
                if ((offset == OffsetKind.Spaces) && tabsOffset.IsMatch(line))
                {
                    return false;
                }
                else
                if ((offset == OffsetKind.Tabs) && spacesOffset.IsMatch(line))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
