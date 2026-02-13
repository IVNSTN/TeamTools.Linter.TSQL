using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDecimalTypeReference : SqlGenericTypeReference<SqlDecimalValueRange>
    {
        public SqlDecimalTypeReference(string typeName, SqlDecimalValueRange size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
        }

        public override bool Equals(SqlTypeReference other)
        {
            return base.Equals(other)
                && other is SqlDecimalTypeReference decRef
                && decRef.Size.Equals(Size);
        }

        public override string ToString()
        {
            return $"{TypeName}({Size.Precision}, {Size.Scale})";
        }

        // docs: https://learn.microsoft.com/en-us/sql/t-sql/data-types/decimal-and-numeric-transact-sql?view=sql-server-ver17
        protected override int GetBytes()
        {
            if (Size.Precision >= 29)
            {
                return 17;
            }

            if (Size.Precision >= 20)
            {
                return 13;
            }

            if (Size.Precision >= 10)
            {
                return 9;
            }

            return 5;
        }
    }
}
