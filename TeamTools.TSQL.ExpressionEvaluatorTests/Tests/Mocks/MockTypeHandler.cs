using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public class MockTypeHandler : ISqlTypeHandler
    {
        private readonly ISqlValueFactory factory;

        public MockTypeHandler(ISqlValueFactory factory)
        {
            this.factory = factory;
        }

        public ISqlValueFactory ValueFactory => GetValueFactory();

        public ICollection<string> SupportedTypes => MockTypes.SupportedTypes;

        public SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
        {
            return from;
        }

        public SqlValue ConvertFrom(SqlValue from, string to)
        {
            return from;
        }

        public ISqlValueFactory GetValueFactory() => factory;

        public bool IsSameTypeSize(SqlTypeReference typeRef, SqlValue value) => false;

        public bool IsTypeSupported(string typeName) => MockTypes.Supports(typeName);

        public SqlTypeReference MakeSqlDataTypeReference(DataTypeReference dataType)
        {
            return MakeSqlDataTypeReference(dataType.GetFullName());
        }

        public SqlTypeReference MakeSqlDataTypeReference(string typeName)
        {
            return new MockSqlTypeReference(typeName, GetValueFactory(), 0);
        }

        public SqlValue MergeTwoEstimates(SqlValue first, SqlValue second)
        {
            return first.TypeReference.MakeUnknownValue();
        }
    }
}
