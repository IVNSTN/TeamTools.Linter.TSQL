using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public sealed class SqlTimeTypeHandler : SqlGenericDateTimeTypeHandler<TimeSpan, SqlTimeOnlyValue>
    {
        private readonly SqlTimeTypeValueFactory typedValueFactory;

        public SqlTimeTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlTimeTypeValueFactory(), typeConverter, violations)
        {
        }

        private SqlTimeTypeHandler(SqlTimeTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
        }

        public SqlTimeTypeValueFactory TimeValueFactory => typedValueFactory;

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => to is SqlDateTimeTypeReference datetimeType ? ConvertFrom(from, datetimeType, forceTargetType) : default;

        public SqlTimeOnlyValue ConvertFrom(SqlValue from, SqlDateTimeTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => typedValueFactory.MakeSqlDataTypeReference(typeName);
    }
}
