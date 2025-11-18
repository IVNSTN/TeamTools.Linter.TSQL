using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class ImplicitTruncationViolation : SqlViolation
    {
        // TODO : The problematic value source and the node where implicit truncation occured
        // are different things and are both needed. Same works for other violations.
        public ImplicitTruncationViolation(int typeSize, int valueSize, string value, SqlValueSource source)
        : base(string.Format("{0} < {1}", typeSize.ToString(), valueSize == int.MaxValue ? "MAX" : valueSize.ToString()), source)
        {
            TypeSize = typeSize;
            ValueSize = valueSize;
            ValueSource = source;
            Value = value;

            Debug.Assert(typeSize >= 0, "type size is wrong");
            Debug.Assert(valueSize >= 0, "value size is wrong");
            Debug.Assert(valueSize > typeSize, "and how can this be a truncation?");
        }

        public string TypeName { get; }

        public int TypeSize { get; }

        public int ValueSize { get; }

        public string Value { get; }

        public SqlValueSource ValueSource { get; }
    }
}
