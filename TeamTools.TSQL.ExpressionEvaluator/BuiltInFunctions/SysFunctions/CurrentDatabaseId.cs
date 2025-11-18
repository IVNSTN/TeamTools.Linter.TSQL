using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    [ExcludeFromCodeCoverage]
    public sealed class CurrentDatabaseId : SqlIntTypeValue
    {
        private static readonly SqlIntValueRange IdRange = new SqlIntValueRange(0, int.MaxValue);

        public CurrentDatabaseId(SqlIntTypeHandler typeHandler, SqlValueSource source)
        : base(typeHandler, new SqlIntTypeReference(typeHandler.IntValueFactory.FallbackTypeName, IdRange, typeHandler.GetValueFactory()), SqlValueKind.Unknown, source)
        { }

        public override SqlIntTypeValue DeepClone()
            => new CurrentDatabaseId(TypeHandler, Source.Clone());
    }
}
