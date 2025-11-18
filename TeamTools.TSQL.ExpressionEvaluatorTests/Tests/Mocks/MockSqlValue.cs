using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public class MockSqlValue : SqlValue
    {
        private readonly SqlTypeReference typeRef;
        private readonly ISqlTypeHandler typeHandler;

        public MockSqlValue(string typeName, SqlValueKind valueKind, SqlValueSource source, ISqlTypeHandler typeHandler)
        : base(typeName, valueKind, source)
        {
            this.typeRef = typeHandler.MakeSqlDataTypeReference(TypeName);
            this.typeHandler = typeHandler;
        }

        public MockSqlValue(SqlTypeReference typeRef, SqlValueKind valueKind, SqlValueSource source, ISqlTypeHandler typeHandler)
        : base(typeRef.TypeName, valueKind, source)
        {
            this.typeRef = typeRef;
            this.typeHandler = typeHandler;
        }

        public int IntValue { get; set; } = 0;

        public string StrValue { get; set; } = "";

        public override ISqlTypeHandler GetTypeHandler() => typeHandler;

        public override SqlTypeReference GetTypeReference() => typeRef;
    }
}
