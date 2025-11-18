using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0304", "FILE_NAME_SQUARE_BRACKETS")]
    internal sealed class FileNameSquareBracketsRule : AbstractRule, IFileLevelRule
    {
        public FileNameSquareBracketsRule() : base()
        {
        }

        public void VerifyFile(string filePath, TSqlFragment sqlFragment = null)
        {
            var fileName = Path.GetFileName(filePath);
            if (!(fileName.Contains('[') || fileName.Contains(']')))
            {
                return;
            }

            HandleFileError();
        }
    }
}
