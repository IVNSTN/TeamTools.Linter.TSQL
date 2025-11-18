using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBigIntTypeValue : SqlGenericValueWithHandler<SqlBigIntTypeHandler, SqlBigIntTypeValue, SqlBigIntValueRange, BigInteger, SqlBigIntTypeValue>
    {
        public SqlBigIntTypeValue(SqlBigIntTypeHandler typeHandler, SqlBigIntTypeReference typeRef, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeRef, valueKind, source)
        {
        }

        public SqlBigIntTypeValue(SqlBigIntTypeHandler typeHandler, SqlBigIntTypeReference typeRef, BigInteger value, SqlValueSource source)
        : base(typeHandler, typeRef, value, source)
        {
        }

        // For cloning
        protected SqlBigIntTypeValue(SqlBigIntTypeValue src, BigInteger value) : base(src, value)
        {
        }

        // For cloning
        protected SqlBigIntTypeValue(SqlBigIntTypeValue src) : base(src)
        {
        }

        public SqlBigIntTypeValue ChangeTo(int newValue, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlBigIntTypeValue ChangeTo(SqlBigIntValueRange newSize, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newSize, source);

        public override SqlBigIntTypeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlBigIntTypeValue(this, Value);
            }

            return new SqlBigIntTypeValue(this);
        }
    }
}
