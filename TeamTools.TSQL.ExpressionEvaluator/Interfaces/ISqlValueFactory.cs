using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface ISqlValueFactory
    {
        SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind);

        SqlValue NewNull(TSqlFragment source);

        SqlValue NewLiteral(string typeName, string value, TSqlFragment source);
    }
}
