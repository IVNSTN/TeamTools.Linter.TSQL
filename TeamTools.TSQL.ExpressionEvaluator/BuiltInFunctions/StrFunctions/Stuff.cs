using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // TODO : very similar to Space class
    public class Stuff : SqlGenericFunctionHandler<Stuff.StuffArgs>
    {
        private static readonly int RequiredArgumentCount = 4;
        private static readonly string FuncName = "STUFF";
        private static readonly SqlIntValueRange ValidCount = new SqlIntValueRange(0, int.MaxValue);

        public Stuff() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<StuffArgs> call)
        {
            return ValidationScenario
                    .For("STR", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.SourceString = s)
                && ValidationScenario
                    .For("START", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.Start = i)
                && ValidationScenario
                    .For("LENGTH", call.RawArgs[2], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.Length = i)
                && ValidationScenario
                    .For("SUBSTR", call.RawArgs[3], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.InsertedSubstring = s);
        }

        protected override string DoEvaluateResultType(CallSignature<StuffArgs> call) => call.ValidatedArgs.SourceString.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<StuffArgs> call)
        {
            var str = call.ValidatedArgs.SourceString;

            if (str.IsNull
            || call.ValidatedArgs.Start.IsNull
            || call.ValidatedArgs.Length.IsNull
            || call.ValidatedArgs.InsertedSubstring.IsNull)
            {
                call.Context.RedundantCall("STR, start pos and length should not be NULL", call.ValidatedArgs.InsertedSubstring.Source);
                return str.TypeReference.MakeNullValue();
            }

            if (str.EstimatedSize == 0)
            {
                call.Context.RedundantCall("STR is empty", call.ValidatedArgs.InsertedSubstring.Source);
                return str;
            }

            if (call.ValidatedArgs.Start.EstimatedSize.Low > ValidCount.High
            || call.ValidatedArgs.Start.EstimatedSize.High < ValidCount.Low
            || call.ValidatedArgs.Start.EstimatedSize.Low > str.EstimatedSize)
            {
                call.Context.InvalidArgument("START", "Start pos is out of source string");
                return str.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.Length.EstimatedSize.Low > ValidCount.High
            || call.ValidatedArgs.Length.EstimatedSize.High < ValidCount.Low)
            {
                call.Context.InvalidArgument("LENGTH", "Length is out of range");
                return str.TypeReference.MakeNullValue();
            }

            if (str.IsPreciseValue
            && call.ValidatedArgs.Start.IsPreciseValue
            && call.ValidatedArgs.Length.IsPreciseValue)
            {
                string result = str.Value
                    .Remove(call.ValidatedArgs.Start.Value - 1, call.ValidatedArgs.Length.Value)
                    .Insert(call.ValidatedArgs.Start.Value - 1, call.ValidatedArgs.InsertedSubstring.Value);

                return str.ChangeTo(result, call.Context.NewSource);
            }

            // TODO : ignore MAXINT-level approximate arguments
            int removedPartLength = call.ValidatedArgs.Start.EstimatedSize.Low + call.ValidatedArgs.Length.EstimatedSize.High <= str.EstimatedSize
                ? call.ValidatedArgs.Length.EstimatedSize.High
                : str.EstimatedSize - call.ValidatedArgs.Start.EstimatedSize.Low;

            int estimateResultLength = str.EstimatedSize
                - removedPartLength
                + call.ValidatedArgs.InsertedSubstring.EstimatedSize;

            if (estimateResultLength > 0)
            {
                return str.ChangeTo(estimateResultLength, call.Context.NewSource);
            }

            return default;
        }

        public class StuffArgs
        {
            public SqlStrTypeValue SourceString { get; set; }

            public SqlIntTypeValue Start { get; set; }

            public SqlIntTypeValue Length { get; set; }

            public SqlStrTypeValue InsertedSubstring { get; set; }
        }
    }
}
