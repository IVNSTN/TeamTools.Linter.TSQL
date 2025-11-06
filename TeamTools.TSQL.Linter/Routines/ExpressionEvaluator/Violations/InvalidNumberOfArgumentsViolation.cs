using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class InvalidNumberOfArgumentsViolation : SqlViolation
    {
        public InvalidNumberOfArgumentsViolation(string functionName, int requiredArgs, int actualArgs, SqlValueSource src)
        : base($"{functionName} expects {requiredArgs} but {actualArgs} provided", src)
        {
            FunctionName = functionName;
            MinArgs = requiredArgs;
            MaxArgs = requiredArgs;
            ActualArgs = actualArgs;
        }

        public InvalidNumberOfArgumentsViolation(string functionName, int minArgs, int maxArgs, int actualArgs, SqlValueSource src)
        : base($"{functionName} accepts {minArgs}-{maxArgs} but {actualArgs} provided", src)
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
