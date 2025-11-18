using System.Text;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // TODO : very similar to Space class
    public class Replicate : SqlGenericFunctionHandler<Replicate.ReplicateStrArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly int MaxLengthToGenerate = 200;
        private static readonly string FuncName = "REPLICATE";
        private static readonly SqlIntValueRange ValidCount = new SqlIntValueRange(0, int.MaxValue);

        public Replicate() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ReplicateStrArgs> call)
        {
            return ValidationScenario
                    .For("STR", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.Substr = s)
                && ValidationScenario
                    .For("COUNT", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.Count = i);
        }

        protected override string DoEvaluateResultType(CallSignature<ReplicateStrArgs> call) => call.ValidatedArgs.Substr.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ReplicateStrArgs> call)
        {
            var str = call.ValidatedArgs.Substr;

            if (str.IsNull)
            {
                call.Context.RedundantCall("STR is NULL", call.ValidatedArgs.Substr.Source);
                return str.TypeReference.MakeNullValue();
            }

            if (str.EstimatedSize == 0)
            {
                call.Context.RedundantCall("STR is empty", call.ValidatedArgs.Substr.Source);
                return str.ChangeTo("", call.Context.NewSource);
            }

            if (call.ValidatedArgs.Count.IsNull)
            {
                call.Context.RedundantCall("COUNT is NULL", call.ValidatedArgs.Substr.Source);
                return str.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.Count.EstimatedSize.Low > ValidCount.High
            || call.ValidatedArgs.Count.EstimatedSize.High < ValidCount.Low)
            {
                call.Context.InvalidArgument("COUNT", "Value is out of range");
                return str.TypeReference.MakeNullValue();
            }

            int estimatedLength = call.ValidatedArgs.Count.EstimatedSize.High * str.EstimatedSize;

            if (call.ValidatedArgs.Count.IsPreciseValue && str.IsPreciseValue
            && estimatedLength <= MaxLengthToGenerate)
            {
                var replicateResult = new StringBuilder(estimatedLength)
                    .Insert(0, str.Value, call.ValidatedArgs.Count.Value)
                    .ToString();

                return str.ChangeTo(replicateResult, call.Context.NewSource);
            }

            return str.ChangeTo(estimatedLength, call.Context.NewSource);
        }

        public class ReplicateStrArgs
        {
            public SqlStrTypeValue Substr { get; set; }

            public SqlIntTypeValue Count { get; set; }
        }
    }
}
