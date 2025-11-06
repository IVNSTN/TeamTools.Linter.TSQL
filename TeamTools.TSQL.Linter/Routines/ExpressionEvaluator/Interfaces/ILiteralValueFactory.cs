using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface ILiteralValueFactory
    {
        SqlValue MakeNull(TSqlFragment src);

        SqlValue Make(Literal src);
    }
}
