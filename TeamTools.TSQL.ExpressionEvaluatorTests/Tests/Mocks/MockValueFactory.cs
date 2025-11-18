using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public class MockValueFactory : ISqlValueFactory
    {
        public ISqlTypeHandler TypeHandler { get; set; }

        public int NewLiteralCalls { get; private set; } = 0;

        public int NewNullCalls { get; private set; } = 0;

        public int NewValueCalls { get; private set; } = 0;

        public int NewCalls => NewLiteralCalls + NewNullCalls + NewValueCalls;

        public SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            NewLiteralCalls++;

            if (!MockTypes.Supports(typeName))
            {
                return default;
            }

            var literalValue = new MockSqlValue(typeName, SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Literal, source), TypeHandler);

            if (typeName == "INT")
            {
                literalValue.IntValue = int.Parse(value);
            }
            else
            {
                literalValue.StrValue = value;
            }

            return literalValue;
        }

        public SqlValue NewNull(TSqlFragment source)
        {
            NewNullCalls++;

            return new MockSqlValue("VARCHAR", SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Literal, source), TypeHandler);
        }

        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind)
        {
            NewValueCalls++;

            if (!MockTypes.Supports(typeRef.TypeName))
            {
                return default;
            }

            return new MockSqlValue(typeRef, valueKind, null, TypeHandler);
        }
    }
}
