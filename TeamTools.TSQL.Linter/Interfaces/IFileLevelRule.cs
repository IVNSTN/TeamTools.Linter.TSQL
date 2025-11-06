using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Interfaces
{
    public interface IFileLevelRule
    {
        void VerifyFile(string filePath, TSqlFragment sqlFragment = null);
    }
}
