using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.Values
{
    public abstract class SqlValue
    {
        protected SqlValue(string typeName, SqlValueKind valueKind, SqlValueSource source)
        {
            Source = source;
            TypeName = typeName;
            ValueKind = valueKind;
            IsPreciseValue = ValueKind != SqlValueKind.Unknown;
            IsNull = ValueKind == SqlValueKind.Null;
        }

        public SqlValueKind ValueKind { get; }

        public string TypeName { get; }

        // FIXME: public set does not seem to be a great idea
        // FIXME: so cloning or immutability???
        public SqlValueSource Source { get; set; }

        public SqlValueSourceKind SourceKind => Source?.SourceKind ?? SqlValueSourceKind.Expression;

        public bool IsPreciseValue { get; }

        public bool IsNull { get; }

        public SqlTypeReference TypeReference => GetTypeReference();

        public abstract ISqlTypeHandler GetTypeHandler();

        public abstract SqlTypeReference GetTypeReference();
    }
}
