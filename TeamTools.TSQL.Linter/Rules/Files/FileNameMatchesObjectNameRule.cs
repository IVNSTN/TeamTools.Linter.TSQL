using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.IO;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0305", "FILE_OBJECT_NAME")]
    internal sealed class FileNameMatchesObjectNameRule : AbstractRule, IFileLevelRule
    {
        // # is fine for filenames however VS prevents from using it
        // so treating it as an illegal symbol as well
        private readonly Regex invalidFilenameChars = new Regex("[<>:\"\\/|?*#]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public FileNameMatchesObjectNameRule() : base()
        {
        }

        public void VerifyFile(string filePath, TSqlFragment node)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            var mainObjectDetector = new MainScriptObjectDetector();

            node.Accept(mainObjectDetector);

            if (string.IsNullOrEmpty(mainObjectDetector.ObjectFullName))
            {
                return;
            }

            if (invalidFilenameChars.IsMatch(mainObjectDetector.ObjectFullName))
            {
                // dont know how to convert invalid chars to appropriate file name
                return;
            }

            if (string.Equals(mainObjectDetector.ObjectFullName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HandleFileError(mainObjectDetector.ObjectFullName);
        }
    }
}
