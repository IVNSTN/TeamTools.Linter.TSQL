using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    public static class TSqlParserFactory
    {
        public static TSqlParser Make(int compatibilityLevel)
            => TSqlParser.CreateParser(CompatibilityConverter.ToSqlVersion(compatibilityLevel), true);
    }
}
