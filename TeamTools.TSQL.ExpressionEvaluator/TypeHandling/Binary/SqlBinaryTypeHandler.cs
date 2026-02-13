using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBinaryTypeHandler : SqlGenericTypeHandler<SqlBinaryTypeValue, int, HexValue>,
        IPlusOperatorHandler
    {
        private readonly SqlBinaryTypeValueFactory typedValueFactory;
        private readonly Func<string, int> getDefaultSize;
        private readonly Func<int, int, int> getTotalSize;
        private readonly Func<HexValue, HexValue, HexValue> getConcat;
        private readonly Func<int, int, int> makeApproximateSum;

        public SqlBinaryTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlBinaryTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlBinaryTypeHandler(SqlBinaryTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
            getDefaultSize = new Func<string, int>(valueFactory.GetDefaultTypeSize);
            getTotalSize = new Func<int, int, int>((a, b) => a + b);
            getConcat = new Func<HexValue, HexValue, HexValue>((a, b) => a + b);

            makeApproximateSum = new Func<int, int, int>(CombineSize);
        }

        public SqlBinaryTypeValueFactory BinaryValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

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

        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                getConcat,
                makeApproximateSum);
        }

        public override SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                return default;
            }

            string typeName = dataType.GetFullName();
            int typeSize = GetTypeSize(dataType);

            if (typeSize <= 0)
            {
                // could not determine valid string size
                return default;
            }

            return BinaryValueFactory.MakeSqlDataTypeReference(typeName, typeSize);
        }

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => BinaryValueFactory.MakeSqlDataTypeReference(typeName, BinaryValueFactory.GetDefaultTypeSize(typeName));

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => to is SqlBinaryTypeReference strType ? ConvertFrom(from, strType, forceTargetType) : default;

        public SqlBinaryTypeValue ConvertFrom(SqlValue from, SqlBinaryTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        protected override int GetTypeSize(DataTypeReference datatype)
        {
            // Binary is very similar to Char
            return SqlStrTypeDefinitionParser.GetTypeSize(datatype, getDefaultSize);
        }
    }
}
