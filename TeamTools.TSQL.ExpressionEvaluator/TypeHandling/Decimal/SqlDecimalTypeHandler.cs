using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDecimalTypeHandler : SqlGenericTypeHandler<SqlDecimalTypeValue, SqlDecimalValueRange, decimal>,
        IPlusOperatorHandler, IMinusOperatorHandler, IMultiplyOperatorHandler, IDivideOperatorHandler,
        IReverseValueSignHandler
    {
        private readonly SqlDecimalTypeValueFactory typedValueFactory;
        private readonly Func<decimal, decimal, decimal> getSum;
        private readonly Func<decimal, decimal, decimal> getMultiply;
        private readonly Func<decimal, decimal, decimal> getSubtract;
        private readonly Func<SqlDecimalValueRange, SqlDecimalValueRange, SqlDecimalValueRange> makeApproxSum;

        static SqlDecimalTypeHandler()
        {
        }

        public SqlDecimalTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlDecimalTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlDecimalTypeHandler(SqlDecimalTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;

            getSum = new Func<decimal, decimal, decimal>((a, b) => a + b);
            getMultiply = new Func<decimal, decimal, decimal>((a, b) => a * b);
            getSubtract = new Func<decimal, decimal, decimal>((a, b) => a - b);

            // TODO : move Precision and Scale calculations to the range class implementation?
            // FIXME : prevent range bounds from falling out of valid Int range
            // FIXME : take max precision and scale
            makeApproxSum = new Func<SqlDecimalValueRange, SqlDecimalValueRange, SqlDecimalValueRange>((sizeA, sizeB) => new SqlDecimalValueRange(
                sizeA.Low == decimal.MinValue || sizeB.Low == decimal.MinValue ? decimal.MinValue : sizeA.Low + sizeB.Low,
                sizeA.High == decimal.MaxValue || sizeB.High == decimal.MaxValue ? decimal.MaxValue : sizeA.High + sizeB.High,
                Math.Max(sizeA.Scale, sizeB.Scale) + Math.Max(sizeA.Precision - sizeA.Scale, sizeB.Precision - sizeB.Scale) + 1,
                Math.Max(sizeA.Scale, sizeB.Scale)));
        }

        public SqlDecimalTypeValueFactory DecimalValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlDecimalValueRange CombineSize(SqlDecimalValueRange a, SqlDecimalValueRange b)
        {
            return new SqlDecimalValueRange(
                Math.Min(a.Low, b.Low),
                Math.Max(a.High, b.High),
                Math.Max(a.Precision, b.Precision),
                Math.Max(a.Scale, b.Scale));
        }

        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                getSum,
                makeApproxSum);
        }

        // TODO : register redundant minus 0
        public SqlValue Subtract(SqlValue minuend, SqlValue subtrahend)
        {
            return Compute(
                minuend,
                subtrahend,
                getSubtract,
                // FIXME : prevent range bounds from falling out of valid Decimal range
                (sizeA, sizeB) => new SqlDecimalValueRange(
                    sizeA.Low == decimal.MinValue || sizeB.High == decimal.MaxValue ? decimal.MinValue : sizeA.Low - sizeB.High,
                    sizeA.High == decimal.MaxValue || sizeB.Low == decimal.MinValue ? decimal.MaxValue : sizeA.High - sizeB.Low,
                    Math.Max(sizeA.Scale, sizeB.Scale) + Math.Max(sizeA.Precision - sizeA.Scale, sizeB.Precision - sizeB.Scale) + 1,
                    Math.Max(sizeA.Scale, sizeB.Scale)));
        }

        // TODO : register redundant multiply by 1
        public SqlValue Multiply(SqlValue multiplicand, SqlValue multiplier)
        {
            return Compute(
                multiplicand,
                multiplier,
                getMultiply,
                (sizeA, sizeB) => new SqlDecimalValueRange(
                    sizeA.Low == decimal.MinValue || sizeB.Low == decimal.MinValue ? decimal.MinValue : sizeA.Low + sizeB.Low,
                    sizeA.High == decimal.MaxValue || sizeB.High == decimal.MaxValue ? decimal.MaxValue : sizeA.High + sizeB.High,
                    sizeA.Precision + sizeB.Precision + 1,
                    sizeA.Scale + sizeB.Scale));
        }

        // TODO : respect expression resulting type. the result must still fit the defined scale
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
                            Violations.RegisterViolation(new IntentionalExceptionViolation(Strings.ViolationDetails_IntentionalException_DivisionByZero, divisor.Source));
                        }
                        else
                        {
                            Violations.RegisterViolation(new DivideByZeroViolation(divisor.Source));
                        }

                        return 0;
                    }

                    return a / b;
                },
                (sizeA, sizeB) => new SqlDecimalValueRange(
                    sizeA.Low == decimal.MinValue || sizeB.High == decimal.MaxValue ? decimal.MinValue : sizeA.Low - sizeB.High,
                    sizeA.High == decimal.MaxValue || sizeB.Low == decimal.MinValue ? decimal.MaxValue : sizeA.High - sizeB.Low,
                    Math.Max(6, sizeA.Scale + sizeB.Precision + 1) + sizeA.Precision - sizeA.Scale + sizeB.Scale,
                    Math.Max(6, sizeA.Scale + sizeB.Precision + 1)));
        }

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => DecimalValueFactory.MakeSqlDataTypeReference(typeName);

        public SqlTypeReference MakeSqlDataTypeReference(string typeName, int precision, int scale)
            => DecimalValueFactory.MakeSqlDataTypeReference(typeName, precision, scale);

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => to is SqlDecimalTypeReference intType ? ConvertFrom(from, intType, forceTargetType) : default;

        public SqlDecimalTypeValue ConvertFrom(SqlValue from, SqlDecimalTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        public SqlValue ReverseSign(SqlValue value)
            => value is SqlDecimalTypeValue decValue ? RevertValueSign(decValue) : default;

        public override SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                return default;
            }

            string typeName = dataType.GetFullName();

            if (dataType is SqlDataTypeReference sdt && sdt.Parameters.Count == 2
            && int.TryParse(sdt.Parameters[0].Value, out int precision)
            && int.TryParse(sdt.Parameters[1].Value, out int scale)
            && precision >= 0 && precision <= TSqlDomainAttributes.MaxDecimalPrecision
            && scale >= 0 && scale <= precision)
            {
                return DecimalValueFactory.MakeSqlDataTypeReference(typeName, precision, scale);
            }

            // making with default precision and scale
            return DecimalValueFactory.MakeSqlDataTypeReference(typeName);
        }

        protected override SqlDecimalValueRange DoMergeTwoEstimatedSizes(SqlDecimalValueRange a, SqlDecimalValueRange b)
        {
            // TODO : verify that 0 <= Scale <= Precision and result size is <= 38
            return new SqlDecimalValueRange(
                Math.Min(a.Low, b.Low),
                Math.Max(a.High, b.High),
                Math.Max(a.Precision, b.Precision),
                Math.Max(a.Scale, b.Scale));
        }

        private SqlDecimalTypeValue RevertValueSign(SqlDecimalTypeValue value)
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

            return ValueFactory.MakeApproximateValue(resultTypeName, SqlDecimalValueRange.RevertRange(value.EstimatedSize), value.Source);
        }
    }
}
