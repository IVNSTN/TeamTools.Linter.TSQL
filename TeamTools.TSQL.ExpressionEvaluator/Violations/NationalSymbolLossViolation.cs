using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class NationalSymbolLossViolation : SqlViolation
    {
        public NationalSymbolLossViolation(string sourceTypeName, SqlValueSource source)
        : base(string.Format(Strings.ViolationDetails_NationalSymbolLoss_SourceMayContainUnicode, sourceTypeName), source)
        {
        }
    }
}
