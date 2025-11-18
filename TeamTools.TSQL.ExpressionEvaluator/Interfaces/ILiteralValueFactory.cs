using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface ILiteralValueFactory
    {
        SqlValue MakeNull(TSqlFragment src);

        SqlValue Make(Literal src);
    }
}
