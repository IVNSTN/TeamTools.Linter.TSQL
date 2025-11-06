using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface ISqlValueFactory
    {
        SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind);

        SqlValue NewNull(TSqlFragment source);

        SqlValue NewLiteral(string typeName, string value, TSqlFragment source);
    }
}
