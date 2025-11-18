using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeHandler : SqlGenericTypeHandler<SqlStrTypeValue, int, string>,
        IPlusOperatorHandler
    {
        private readonly SqlStrTypeValueFactory typedValueFactory;
        private readonly Func<string, int> getDefaultSize;
        private readonly Func<int, int, int> getTotalSize;
        private readonly Func<string, string, string> getConcat;
        private readonly Func<int, int, int> makeApproximateSum;

        public SqlStrTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlStrTypeValueFactory(), typeConverter, violations)
        {
        }

        protected SqlStrTypeHandler(SqlStrTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;
            getDefaultSize = new Func<string, int>(valueFactory.GetDefaultTypeSize);
            getTotalSize = new Func<int, int, int>((a, b) => a + b);
            getConcat = new Func<string, string, string>((a, b) =>
            {
                if (string.IsNullOrEmpty(a))
                {
                    return b;
                }

                if (string.IsNullOrEmpty(b))
                {
                    return a;
                }

                return $"{a}{b}";
            });

            makeApproximateSum = new Func<int, int, int>(CombineSize);
        }

        public SqlStrTypeValueFactory StrValueFactory => typedValueFactory;

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

            return StrValueFactory.MakeSqlTypeReference(typeName, typeSize);
        }

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => StrValueFactory.MakeSqlTypeReference(typeName, StrValueFactory.GetDefaultTypeSize(typeName));

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => to is SqlStrTypeReference strType ? ConvertFrom(from, strType, forceTargetType) : default;

        public SqlStrTypeValue ConvertFrom(SqlValue from, SqlStrTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        protected override int GetTypeSize(DataTypeReference datatype)
        {
            return SqlStrTypeDefinitionParser.GetTypeSize(datatype, getDefaultSize);
        }
    }
}
