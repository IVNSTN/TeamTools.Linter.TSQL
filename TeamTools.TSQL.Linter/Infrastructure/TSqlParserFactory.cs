using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    public class TSqlParserFactory
    {
        public TSqlParser Make(int compatibilityLevel)
        {
            return TSqlParser.CreateParser(CompatibilityConverter.ToSqlVersion(compatibilityLevel), true);
        }
    }
}
