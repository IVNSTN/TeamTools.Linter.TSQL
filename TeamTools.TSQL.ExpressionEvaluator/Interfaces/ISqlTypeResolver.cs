using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface ISqlTypeResolver
    {
        SqlTypeReference ResolveType(DataTypeReference dataType);

        SqlTypeReference ResolveType(string typeName);

        ISqlTypeHandler ResolveTypeHandler(string typeName);

        bool IsSupportedType(string typeName);
    }
}
