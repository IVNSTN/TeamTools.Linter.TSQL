using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlTypeReference : IEquatable<SqlTypeReference>, IComparable<SqlTypeReference>
    {
        private readonly ISqlValueFactory valueFactory;

        protected SqlTypeReference(string typeName, ISqlValueFactory valueFactory)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            // TODO : ValueFactory or TypeHandler?
            this.valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentOutOfRangeException(nameof(typeName));
            }
        }

        public string TypeName { get; }

        public SqlValue MakeValue(SqlValueKind valueKind)
        {
            if (valueKind == SqlValueKind.Precise)
            {
                throw new ArgumentOutOfRangeException(nameof(valueKind), "For precise values use specific factory directly");
            }

            return valueFactory.NewValue(this, valueKind);
        }

        public SqlValue MakeUnknownValue() => MakeValue(SqlValueKind.Unknown);

        public SqlValue MakeNullValue() => MakeValue(SqlValueKind.Null);

        public virtual bool Equals(SqlTypeReference other)
            => IsOfSameType(other);

        public override string ToString() => TypeName;

        public abstract int CompareTo(SqlTypeReference other);

        protected bool IsOfSameType(SqlTypeReference other)
        {
            return other.GetType().IsAssignableFrom(GetType())
                && string.Equals(other.TypeName, TypeName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
