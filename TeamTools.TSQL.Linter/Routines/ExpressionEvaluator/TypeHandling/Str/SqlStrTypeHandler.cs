using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeHandler : SqlGenericTypeHandler<SqlStrTypeValue, int, string>,
        IPlusOperatorHandler
    {
        private readonly SqlStrTypeValueFactory typedValueFactory;

        public SqlStrTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlStrTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlStrTypeHandler(SqlStrTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
        }

        public SqlStrTypeValueFactory StrValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlStrTypeValue Convert(SqlValue src)
            => TypeConverter.ImplicitlyConvert<SqlStrTypeValue>(src);

        public override int CombineSize(int a, int b)
        {
            // unknown string length
            if (a < 0 || b < 0)
            {
                return -1;
            }

            if (a == int.MaxValue || b == int.MaxValue)
            {
                // MAX cannot be greater than MAX
                return int.MaxValue;
            }

            return a + b;
        }

        public override SqlStrTypeValue ChangeValueTo(SqlStrTypeValue old, string newValue, SqlValueSource source)
             => StrValueFactory.MakePreciseValue(old.TypeName, newValue, source);

        public override SqlStrTypeValue ChangeValueTo(SqlStrTypeValue old, int newSize, SqlValueSource source)
            => StrValueFactory.MakeApproximateValue(old.TypeName, newSize, source);

        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                (a, b) => a + b);
        }

        public override SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                return default;
            }

            string typeName = dataType.Name.GetFullName();
            int typeSize = GetTypeSize(dataType);

            if (typeSize <= 0)
            {
                // could not determine valid string size
                return default;
            }

            return StrValueFactory.MakeSqlTypeReference(typeName, typeSize);
        }

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => StrValueFactory.MakeSqlTypeReference(typeName, StrValueFactory.GetDefaultTypeSize(typeName));

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => (to is SqlStrTypeReference strType) ? ConvertFrom(from, strType, forceTargetType) : default;

        public SqlStrTypeValue ConvertFrom(SqlValue from, SqlStrTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        protected override int GetTypeSize(DataTypeReference datatype)
        {
            return SqlStrTypeDefinitionParser.GetTypeSize(datatype, typeName => ValueFactory.GetDefaultTypeSize(typeName));
        }
    }
}
