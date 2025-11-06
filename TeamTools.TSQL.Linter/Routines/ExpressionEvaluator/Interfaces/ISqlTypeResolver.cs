using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface ISqlTypeResolver
    {
        SqlTypeReference ResolveType(DataTypeReference dataType);

        SqlTypeReference ResolveType(string typeName);

        ISqlTypeHandler ResolveTypeHandler(string typeName);

        bool IsSupportedType(string typeName);
    }
}
