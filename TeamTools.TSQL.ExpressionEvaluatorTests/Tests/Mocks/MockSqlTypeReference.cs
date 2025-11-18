using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public sealed class MockSqlTypeReference : SqlTypeReference
    {
        public MockSqlTypeReference(string typeName, ISqlValueFactory valueFactory, int dummy) : base(typeName, valueFactory)
        {
            Dummy = dummy;
        }

        public int Dummy { get; }

        public override int CompareTo(SqlTypeReference other) => 0;
    }
}
