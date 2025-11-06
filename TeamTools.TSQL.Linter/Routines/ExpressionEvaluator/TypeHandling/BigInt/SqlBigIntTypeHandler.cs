using System.Numerics;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlBigIntTypeHandler : SqlGenericTypeHandler<SqlBigIntTypeValue, SqlBigIntValueRange, BigInteger>,
        IPlusOperatorHandler, IMinusOperatorHandler, IMultiplyOperatorHandler, IDivideOperatorHandler,
        IReverseValueSignHandler
    {
        private readonly SqlBigIntTypeValueFactory typedValueFactory;

        public SqlBigIntTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlBigIntTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlBigIntTypeHandler(SqlBigIntTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
        }

        public SqlBigIntTypeValueFactory BigIntValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlBigIntTypeValue Convert(SqlValue src)
        {
            return TypeConverter.ImplicitlyConvert<SqlBigIntTypeValue>(src);
        }

        public override SqlBigIntValueRange CombineSize(SqlBigIntValueRange a, SqlBigIntValueRange b)
        {
            BigInteger rangeMin = a.Low < b.Low ? a.Low : b.Low;
            BigInteger rangeMax = a.High > b.High ? a.High : b.High;

            return new SqlBigIntValueRange(rangeMin, rangeMax);
        }

        public override SqlBigIntTypeValue ChangeValueTo(SqlBigIntTypeValue old, BigInteger newValue, SqlValueSource source)
            => BigIntValueFactory.MakePreciseValue(old.TypeName, newValue, source);

        public override SqlBigIntTypeValue ChangeValueTo(SqlBigIntTypeValue old, SqlBigIntValueRange newSize, SqlValueSource source)
            => BigIntValueFactory.MakeApproximateValue(old.TypeName, newSize, source);

        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                (a, b) => a + b,
                (sizeA, sizeB) => new SqlBigIntValueRange(sizeA.Low + sizeB.Low, sizeA.High + sizeB.High));
        }

        public SqlValue Subtract(SqlValue minuend, SqlValue subtrahend)
        {
            return Compute(
                minuend,
                subtrahend,
                (a, b) => a - b,
                (sizeA, sizeB) => new SqlBigIntValueRange(sizeA.Low - sizeB.High, sizeA.High - sizeB.Low));
        }

        public SqlValue Multiply(SqlValue multiplicand, SqlValue multiplier)
        {
            return Compute(
                multiplicand,
                multiplier,
                (a, b) => a * b);
        }

        // TODO : respect expression resulting type
        public SqlValue Divide(SqlValue dividend, SqlValue divisor)
        {
            return Compute(
                dividend,
                divisor,
                (a, b) =>
                {
                    if (b == 0)
                    {
                        // Expressions like 1/0 are sometimes used to raise error intentionally
                        if (dividend.SourceKind == SqlValueSourceKind.Literal && divisor.SourceKind == SqlValueSourceKind.Literal
                        && (a == 0 || a == 1))
                        {
                            Violations.RegisterViolation(new IntentionalExceptionViolation("Division by zero", divisor.Source));
                        }
                        else
                        {
                            Violations.RegisterViolation(new DivideByZeroViolation(divisor.Source));
                        }

                        return 0;
                    }

                    return a / b;
                });
        }

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => BigIntValueFactory.MakeSqlTypeReference(typeName);

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => (to is SqlBigIntTypeReference bigintType) ? ConvertFrom(from, bigintType, forceTargetType) : default;

        public SqlBigIntTypeValue ConvertFrom(SqlValue from, SqlBigIntTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        public SqlValue ReverseSign(SqlValue value)
            => (value is SqlBigIntTypeValue bigintValue) ? RevertValueSign(bigintValue) : default;

        private SqlBigIntTypeValue RevertValueSign(SqlBigIntTypeValue value)
        {
            if (value is null)
            {
                return default;
            }

            if (value.IsNull)
            {
                return value;
            }

            if (value.IsPreciseValue && value.Value == 0)
            {
                return value;
            }

            string resultTypeName = value.TypeName;

            if (value.IsPreciseValue)
            {
                return ValueFactory.MakePreciseValue(resultTypeName, -value.Value, value.Source);
            }

            return ValueFactory.MakeApproximateValue(resultTypeName, SqlBigIntValueRange.RevertRange(value.EstimatedSize), value.Source);
        }
    }
}
