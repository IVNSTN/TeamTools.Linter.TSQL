using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
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
