using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDateTimeTypeReference : SqlGenericTypeReference<SqlDateTimeValueRange>
    {
        public SqlDateTimeTypeReference(string typeName, SqlDateTimeValueRange size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        { }

        public static DateTime MinSqlValue { get; } = new DateTime(1900, 1, 1);

        [ExcludeFromCodeCoverage]
        protected override int GetBytes()
        {
            switch (TypeName)
            {
                case "DATE": return 3;
                case "TIME": return 5;
                case "SMALLDATETIME": return 4;
            }

            // TODO : respect DATETIME2 precision
            return 8;
        }
    }
}
