using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0301", "FILE_ENCODING")]
    internal class AllowedEncodingRule : AbstractRule, IFileLevelRule
    {
        private readonly EncodingDetector encodingDetector = new EncodingDetector();

        public AllowedEncodingRule() : base()
        {
        }

        public void VerifyFile(string filePath, TSqlFragment sqlFragment = null)
        {
            using (Stream file = File.OpenRead(filePath))
            {
                VerifyFileEncoding(file);
            }
        }

        protected virtual void VerifyFileEncoding(Stream source)
        {
            Encoding enc = encodingDetector.GetFileEncoding(source);

            if (enc == Encoding.UTF8)
            {
                return;
            }

            HandleFileError();
        }
    }
}
