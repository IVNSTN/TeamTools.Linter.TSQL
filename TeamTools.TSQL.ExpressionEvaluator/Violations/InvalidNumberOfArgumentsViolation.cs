using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class InvalidNumberOfArgumentsViolation : SqlViolation
    {
        public InvalidNumberOfArgumentsViolation(string functionName, int requiredArgs, int actualArgs, SqlValueSource src)
        : base(string.Format(Strings.ViolationDetails_InvalidNumberOfArgumentsViolation_CountMismatch, functionName, requiredArgs, actualArgs), src)
        {
            FunctionName = functionName;
            MinArgs = requiredArgs;
            MaxArgs = requiredArgs;
            ActualArgs = actualArgs;
        }

        public InvalidNumberOfArgumentsViolation(string functionName, int minArgs, int maxArgs, int actualArgs, SqlValueSource src)
        : base(string.Format(Strings.ViolationDetails_InvalidNumberOfArgumentsViolation_CountOutOfRange, functionName, minArgs, maxArgs, actualArgs), src)
        {
            FunctionName = functionName;
            MinArgs = minArgs;
            MaxArgs = maxArgs;
            ActualArgs = actualArgs;
        }

        public string FunctionName { get; }

        public int MinArgs { get; }

        public int MaxArgs { get; }

        public int ActualArgs { get; }
    }
}
