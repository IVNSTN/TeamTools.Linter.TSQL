using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter
{
    internal interface IScriptAnalysisServiceProvider
    {
        T GetService<T>(TSqlFragment script)
        where T : class;
    }
}
