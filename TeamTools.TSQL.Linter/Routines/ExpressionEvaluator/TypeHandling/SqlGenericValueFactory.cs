using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericValueFactory<TValue, TSize, TPreciseValue>
    where TValue : SqlValue
    where TSize : IComparable<TSize>
    where TPreciseValue : IComparable<TPreciseValue>
    {
        public SqlGenericValueFactory(string fallbackTypeName)
        {
            FallbackTypeName = fallbackTypeName ?? throw new ArgumentNullException(nameof(fallbackTypeName));

            if (string.IsNullOrEmpty(fallbackTypeName))
            {
                throw new ArgumentOutOfRangeException(nameof(fallbackTypeName));
            }
        }

        public ICollection<string> SupportedTypes => GetSupportedTypes();

        public string FallbackTypeName { get; }

        public abstract TValue MakePreciseValue(string typeName, TPreciseValue value, SqlValueSource source);

        public abstract TValue MakeApproximateValue(string typeName, TSize estimatedSize, SqlValueSource source);

        public abstract TValue MakeNullValue(string typeName, SqlValueSource source);

        public abstract TValue MakeUnknownValue(string typeName);

        public TValue MakeNull(TSqlFragment source)
        {
            return MakeNullValue(FallbackTypeName, new SqlValueSource(SqlValueSourceKind.Literal, source));
        }

        public TValue MakeLiteral(string typeName, TPreciseValue value, TSqlFragment source)
        {
            return MakePreciseValue(typeName, value, new SqlValueSource(SqlValueSourceKind.Literal, source));
        }

        public bool IsTypeSupported(string typeName)
            => !string.IsNullOrEmpty(typeName) && SupportedTypes.Contains(typeName);

        public abstract TSize GetDefaultTypeSize(string typeName);

        protected abstract ICollection<string> GetSupportedTypes();
    }
}
