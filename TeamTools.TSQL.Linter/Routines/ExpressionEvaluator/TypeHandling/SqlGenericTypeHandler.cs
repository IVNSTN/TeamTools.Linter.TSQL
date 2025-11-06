using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericTypeHandler<TValue, TSize, TPreciseValue> : ISqlTypeHandler
    where TValue : SqlGenericValue<TSize, TPreciseValue>
    where TSize : IComparable<TSize>
    where TPreciseValue : IComparable<TPreciseValue>
    {
        private readonly SqlGenericValueFactory<TValue, TSize, TPreciseValue> valueFactory;
        private readonly ISqlTypeConverter typeConverter;
        private readonly IViolationRegistrar violations;

        public SqlGenericTypeHandler(
            SqlGenericValueFactory<TValue, TSize, TPreciseValue> valueFactory,
            ISqlTypeConverter typeConverter,
            IViolationRegistrar violations)
        {
            this.valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            this.typeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
            this.violations = violations ?? throw new ArgumentNullException(nameof(violations));
        }

        public IViolationRegistrar Violations => violations;

        public ISqlTypeConverter TypeConverter => typeConverter;

        ISqlValueFactory ISqlTypeHandler.ValueFactory => GetValueFactory();

        public ICollection<string> SupportedTypes => ValueFactory.SupportedTypes;

        protected SqlGenericValueFactory<TValue, TSize, TPreciseValue> ValueFactory => valueFactory;

        public abstract TValue Convert(SqlValue src);

        public abstract TSize CombineSize(TSize a, TSize b);

        public abstract TValue ChangeValueTo(TValue old, TPreciseValue newValue, SqlValueSource source);

        public abstract TValue ChangeValueTo(TValue old, TSize newSize, SqlValueSource source);

        public abstract SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false);

        public abstract SqlValue ConvertFrom(SqlValue from, string to);

        public bool IsTypeSupported(string typeName) => ValueFactory.IsTypeSupported(typeName);

        public abstract ISqlValueFactory GetValueFactory();

        public SqlValue MergeTwoEstimates(SqlValue first, SqlValue second)
            => MergeEstimation(Convert(first), Convert(second));

        public TValue MergeEstimation(TValue first, TValue second)
        {
            if (first is null || second is null)
            {
                return default;
            }

            string resultType = first.TypeName;
            // TODO : not sure
            if (!string.Equals(first.TypeName, second.TypeName, StringComparison.OrdinalIgnoreCase))
            {
                resultType = ValueFactory.FallbackTypeName;
            }

            var newEstimate = first.EstimatedSize;
            // if either of them is a variable then variable type from definition
            // should be used for merge
            if (first.IsNull && first.SourceKind != SqlValueSourceKind.Variable)
            {
                newEstimate = second.EstimatedSize;
            }
            else if (second.IsNull && second.SourceKind != SqlValueSourceKind.Variable)
            {
                newEstimate = first.EstimatedSize;
            }
            else
            {
                newEstimate = DoMergeTwoEstimatedSizes(first.EstimatedSize, second.EstimatedSize);
            }

            // TODO : fix node for SqlValueSource or don't pass SqlValueSource to the factory method
            return ValueFactory.MakeApproximateValue(
                resultType,
                newEstimate,
                new SqlValueSource(SqlValueSourceKind.Expression, first.Source?.Node));
        }

        public virtual SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                // e.g. CURSOR
                return default;
            }

            string typeName = dataType.Name.GetFullName();

            return MakeSqlDataTypeReference(typeName);
        }

        public abstract SqlTypeReference MakeSqlDataTypeReference(string typeName);

        protected virtual TSize GetTypeSize(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                // e.g. CURSOR
                return default;
            }

            return ValueFactory.GetDefaultTypeSize(dataType.Name.GetFullName());
        }

        protected virtual TSize DoMergeTwoEstimatedSizes(TSize a, TSize b)
        {
            return a.CompareTo(b) < 0 ? b : a;
        }

        // TODO : respect type precedence
        // TODO : invoke for estimated value ranges to return new range
        protected TValue Compute(
            SqlValue srcValueA,
            SqlValue srcValueB,
            Func<TPreciseValue, TPreciseValue, TPreciseValue> compute,
            Func<TSize, TSize, TSize> approximate = null)
        {
            // conversions should come first to detect possible
            // implicit conversion failure
            var a = Convert(srcValueA);
            var b = Convert(srcValueB);

            if (a == null || b == null)
            {
                return default;
            }

            if (a.IsNull)
            {
                return a;
            }

            if (b.IsNull)
            {
                return b;
            }

            if (a.IsPreciseValue && b.IsPreciseValue)
            {
                TPreciseValue computationResult = compute.Invoke(a.Value, b.Value);

                SqlValueSource resultSource;
                if (srcValueA.SourceKind == SqlValueSourceKind.Literal && srcValueB.SourceKind == SqlValueSourceKind.Literal)
                {
                    resultSource = srcValueA.Source;
                }
                else
                {
                    resultSource = new SqlValueSource(SqlValueSourceKind.Expression, srcValueB.Source?.Node);
                }

                return ValueFactory.MakePreciseValue(a.TypeName, computationResult, resultSource);
            }

            if (approximate is null)
            {
                approximate = CombineSize;
            }

            return ChangeValueTo(
                a,
                approximate(a.EstimatedSize, b.EstimatedSize),
                new SqlValueSource(SqlValueSourceKind.Expression, a.Source?.Node));
        }
    }
}
