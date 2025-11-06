namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public abstract class SqlValue
    {
        public SqlValue(string typeName, SqlValueKind valueKind, SqlValueSource source)
        {
            Source = source;
            TypeName = typeName;
            ValueKind = valueKind;
        }

        public SqlValueKind ValueKind { get; }

        public string TypeName { get; }

        // FIXME: public set does not seem to be a great idea
        // FIXME: so cloning or immutability???
        public SqlValueSource Source { get; set; }

        public SqlValueSourceKind SourceKind => Source?.SourceKind ?? SqlValueSourceKind.Expression;

        public bool IsPreciseValue => ValueKind != SqlValueKind.Unknown;

        public bool IsNull => ValueKind == SqlValueKind.Null;

        public SqlTypeReference TypeReference => GetTypeReference();

        public abstract ISqlTypeHandler GetTypeHandler();

        public abstract SqlTypeReference GetTypeReference();
    }
}
