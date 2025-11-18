using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0305", "FILE_OBJECT_NAME")]
    internal sealed class FileNameMatchesObjectNameRule : ScriptAnalysisServiceConsumingRule, IFileLevelRule
    {
        // '#' is fine for filenames however VS prevents from using it
        // so treating it as an illegal symbol as well
        private static readonly HashSet<char> InvalidFileNameCharsSet = new HashSet<char>
        {
            '\\', '/', ':', '*', '?', '"', '<', '>', '|', '#',
        };

        public FileNameMatchesObjectNameRule() : base()
        {
        }

        public void VerifyFile(string filePath, TSqlFragment node)
        {
            var mainObjectDetector = GetService<MainScriptObjectDetector>(node);

            if (string.IsNullOrEmpty(mainObjectDetector.ObjectFullName))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (fileName.Equals(mainObjectDetector.ObjectFullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (ContainsInvalidFileNameChars(mainObjectDetector.ObjectFullName))
            {
                // dont know how to convert invalid chars to appropriate file name
                return;
            }

            HandleFileError(mainObjectDetector.ObjectFullName);
        }

        private static bool ContainsInvalidFileNameChars(string fileName)
        {
            int n = fileName.Length;
            for (int i = 0; i < n; i++)
            {
                if (InvalidFileNameCharsSet.Contains(fileName[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
