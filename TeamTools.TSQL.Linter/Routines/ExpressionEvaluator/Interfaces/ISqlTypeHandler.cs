using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
