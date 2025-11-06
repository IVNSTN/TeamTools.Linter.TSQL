using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlIntTypeHandler : SqlGenericTypeHandler<SqlIntTypeValue, SqlIntValueRange, int>,
        IPlusOperatorHandler, IMinusOperatorHandler, IMultiplyOperatorHandler, IDivideOperatorHandler,
        IReverseValueSignHandler
    {
        private static readonly ICollection<string> UnsignedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly SqlIntTypeValueFactory typedValueFactory;

        static SqlIntTypeHandler()
        {
            UnsignedTypes.Add("dbo.BIT");
            UnsignedTypes.Add("dbo.TINYINT");
        }

        public SqlIntTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlIntTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlIntTypeHandler(SqlIntTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
        }

        public SqlIntTypeValueFactory IntValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlIntTypeValue Convert(SqlValue src)
            => TypeConverter.ImplicitlyConvert<SqlIntTypeValue>(src);

        public override SqlIntValueRange CombineSize(SqlIntValueRange a, SqlIntValueRange b)
        {
            int rangeMin = a.Low < b.Low ? a.Low : b.Low;
            int rangeMax = a.High > b.High ? a.High : b.High;

            return new SqlIntValueRange(rangeMin, rangeMax);
        }

        public override SqlIntTypeValue ChangeValueTo(SqlIntTypeValue old, int newValue, SqlValueSource source)
            => IntValueFactory.MakePreciseValue(old.TypeName, newValue, source);

        public override SqlIntTypeValue ChangeValueTo(SqlIntTypeValue old, SqlIntValueRange newSize, SqlValueSource source)
            => IntValueFactory.MakeApproximateValue(old.TypeName, newSize, source);

        // TODO : if they are both TINYINT then result should be TINYINT too
        // and if operation result is out of TINIYNT bounds
        // then OutOfrange violation should be registered
        // same for subtraction
        // TODO : register redundant plus 0
        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                (a, b) => a + b,
                // FIXME : prevent range bounds from falling out of valid Int range
                (sizeA, sizeB) => new SqlIntValueRange(
                    sizeA.Low == int.MinValue || sizeB.Low == int.MinValue ? int.MinValue : sizeA.Low + sizeB.Low,
                    sizeA.High == int.MaxValue || sizeB.High == int.MaxValue ? int.MaxValue : sizeA.High + sizeB.High));
        }

        // TODO : register redundant minus 0
        public SqlValue Subtract(SqlValue minuend, SqlValue subtrahend)
        {
            return Compute(
                minuend,
                subtrahend,
                (a, b) => a - b,
                // FIXME : prevent range bounds from falling out of valid Int range
                (sizeA, sizeB) => new SqlIntValueRange(
                    sizeA.Low == int.MinValue || sizeB.High == int.MaxValue ? int.MinValue : sizeA.Low - sizeB.High,
                    sizeA.High == int.MaxValue || sizeB.Low == int.MinValue ? int.MaxValue : sizeA.High - sizeB.Low));
        }

        // TODO : register redundant multiply by 1
        public SqlValue Multiply(SqlValue multiplicand, SqlValue multiplier)
        {
            return Compute(
                multiplicand,
                multiplier,
                (a, b) => a * b);
        }

        // TODO : respect expression resulting type
        // TODO : register redundant divide by 1
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
            => IntValueFactory.MakeSqlTypeReference(typeName);

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => (to is SqlIntTypeReference intType) ? ConvertFrom(from, intType, forceTargetType) : default;

        public SqlIntTypeValue ConvertFrom(SqlValue from, SqlIntTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        public SqlValue ReverseSign(SqlValue value)
            => (value is SqlIntTypeValue intValue) ? RevertValueSign(intValue) : default;

        protected override SqlIntValueRange DoMergeTwoEstimatedSizes(SqlIntValueRange a, SqlIntValueRange b)
        {
            return new SqlIntValueRange(
                a.Low < b.Low ? a.Low : b.Low,
                a.High > b.High ? a.High : b.High);
        }

        private SqlIntTypeValue RevertValueSign(SqlIntTypeValue value)
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
            if (UnsignedTypes.Contains(resultTypeName))
            {
                resultTypeName = IntValueFactory.FallbackTypeName;
            }

            if (value.IsPreciseValue)
            {
                return ValueFactory.MakePreciseValue(resultTypeName, -value.Value, value.Source);
            }

            return ValueFactory.MakeApproximateValue(resultTypeName, SqlIntValueRange.RevertRange(value.EstimatedSize), value.Source);
        }
    }
}
