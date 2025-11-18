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
        private static readonly Regex MixedOffset = new Regex(@"^(([ ]+\t+)|(\t+[ ]+))+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpacesOffset = new Regex(@"^[ ]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TabsOffset = new Regex(@"^\t+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly int MaxViolationsPerFile = 3;

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

        protected override void ValidateScript(TSqlScript node)
        {
            OffsetKind offset = OffsetKind.Unknown;

            CheckTabsAndSpaces(new StringReader(node.GetFragmentText()), node.ScriptTokenStream[node.FirstTokenIndex].Line, ref offset);
        }

        protected virtual void CheckTabsAndSpaces(TextReader reader, int startLine, ref OffsetKind offset)
        {
            string line;
            int lineNumber = startLine - 1;
            int violationCount = 0;

            while ((violationCount < MaxViolationsPerFile) && (line = reader.ReadLine()) != null)
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

        private static bool CheckTabsAndSpaces(string line, ref OffsetKind offset)
        {
            if (MixedOffset.IsMatch(line))
            {
                return false;
            }
            else if (offset == OffsetKind.Unknown)
            {
                if (SpacesOffset.IsMatch(line))
                {
                    offset = OffsetKind.Spaces;
                }
                else if (TabsOffset.IsMatch(line))
                {
                    offset = OffsetKind.Tabs;
                }

                return true;
            }
            else
            {
                if ((offset == OffsetKind.Spaces) && TabsOffset.IsMatch(line))
                {
                    return false;
                }
                else
                if ((offset == OffsetKind.Tabs) && SpacesOffset.IsMatch(line))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
