using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Substring : OriginalStringPartHandler
    {
        private static readonly string FuncName = "SUBSTRING";
        // TODO : in SQLSERVER 2025 the Length arg is optional
        private static readonly int RequiredArgumentCount = 3;

        public Substring() : base(FuncName, RequiredArgumentCount)
        {
        }

        // START can be negative (no exception), LENGTH cannot (exception)
        public override bool ValidateArgumentValues(CallSignature<StrPartArgs> call)
        {
            return (ValidationScenario
                    .For("STR", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.Text = s)
                    // if we could not parse source but parsed length
                    // then we can estimate to approximate string size
                    || true)
                && ValidationScenario
                    .For("START", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.Start = i)
                && ValidationScenario
                    .For("LENGTH", call.RawArgs[2], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.ValidateNonNegativeInt)
                    .Then(i => call.ValidatedArgs.Length = i);
        }

        protected override int DoEvaluateStartValue(SqlIntTypeValue start)
        {
            if (start is null)
            {
                return 0;
            }

            // taking minimum start position
            return start.IsPreciseValue ? start.Value : start.EstimatedSize.Low;
        }

        protected override string TakeStringPartFrom(string srcValue, int startValue, int lenValue)
        {
            if (startValue <= 0)
            {
                // SUBSTRING in TSQL supports negative start and evaluates
                // result for start = -1 and length = 3 as first char of the source string
                lenValue += startValue - 1;
                startValue = 1;
            }

            if (lenValue <= 0)
            {
                // TODO : report RedundantCall?
                return default;
            }

            // SUBSTRING start pos is 1-based whereas string.Substring start is 0-based
            return srcValue.Substring(startValue - 1, lenValue);
        }
    }
}
