using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    [ExcludeFromCodeCoverage]
    public class ArgumentValidation
    {
        public ArgumentValidation(string argumentName, SqlFunctionArgument arg, EvaluationContext context)
        {
            ArgumentName = argumentName;
            Arg = arg;
            Context = context;
        }

        public string ArgumentName { get; }

        public SqlFunctionArgument Arg { get; }

        public EvaluationContext Context { get; }
    }
}
