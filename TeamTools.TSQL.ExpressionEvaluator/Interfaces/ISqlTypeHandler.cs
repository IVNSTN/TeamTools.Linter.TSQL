using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface ISqlTypeHandler
    {
        ISqlValueFactory ValueFactory { get; }

        ICollection<string> SupportedTypes { get; }

        bool IsTypeSupported(string typeName);

        SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType);

        SqlTypeReference MakeSqlDataTypeReference(string typeName);

        SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false);

        SqlValue ConvertFrom(SqlValue from, string to);

        SqlValue MergeTwoEstimates(SqlValue first, SqlValue second);
    }
}
