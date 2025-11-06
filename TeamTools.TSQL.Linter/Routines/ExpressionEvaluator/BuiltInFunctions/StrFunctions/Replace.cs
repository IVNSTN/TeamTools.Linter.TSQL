using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Replace : SqlGenericFunctionHandler<Replace.ReplaceStrArgs>
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly int MaxApproximationLength = 8000;
        private static readonly string FuncName = "REPLACE";

        public Replace() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ReplaceStrArgs> call)
        {
            return ValidationScenario
                .For("STR", call.RawArgs[0], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.SourceString = s)
            && ValidationScenario
                .For("SEARCHED", call.RawArgs[1], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.Searched = s)
            && ValidationScenario
                .For("REPLACEMENT", call.RawArgs[2], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.Replacement = s);
        }

        protected override string DoEvaluateResultType(CallSignature<ReplaceStrArgs> call)
            => call.ValidatedArgs.SourceString.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ReplaceStrArgs> call)
        {
            var str = call.ValidatedArgs.SourceString;

            if (str.IsNull || str.EstimatedSize == 0)
            {
                call.Context.RedundantCall("STR is NULL or empty", call.ValidatedArgs.SourceString.Source);
                // no change
                return str;
            }

            if (call.ValidatedArgs.Searched.IsNull || call.ValidatedArgs.Replacement.IsNull)
            {
                call.Context.RedundantCall("Args should not be NULL", call.ValidatedArgs.Searched.Source);
                return str.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.Searched.EstimatedSize == 0
            || (call.ValidatedArgs.Searched.IsPreciseValue && call.ValidatedArgs.Searched.Value.Length == 0))
            {
                call.Context.RedundantCall("SEARCHED is empty", call.ValidatedArgs.Searched.Source);
                // no change
                return str;
            }

            if (call.ValidatedArgs.SourceString.IsPreciseValue
            && call.ValidatedArgs.Searched.IsPreciseValue
            && call.ValidatedArgs.Replacement.IsPreciseValue)
            {
                if (call.ValidatedArgs.SourceString.Value.IndexOf(call.ValidatedArgs.Searched.Value) < 0)
                {
                    call.Context.RedundantCall("Source string does not contain searched elements", call.ValidatedArgs.Searched.Source);
                    // no change
                    return str;
                }

                if (string.Equals(call.ValidatedArgs.Searched.Value, call.ValidatedArgs.Replacement.Value, System.StringComparison.OrdinalIgnoreCase))
                {
                    // no point in replacement
                    call.Context.RedundantCall("Searched and replaced elements are the same", call.ValidatedArgs.Searched.Source);
                    // no change
                    return str;
                }

                var result = str.Value.Replace(call.ValidatedArgs.Searched.Value, call.ValidatedArgs.Replacement.Value);
                return str.ChangeTo(result, call.Context.NewSource);
            }

            // approximation
            int estimatedLengthChange = call.ValidatedArgs.Replacement.EstimatedSize - call.ValidatedArgs.Searched.EstimatedSize;

            int resultSize = str.EstimatedSize;

            // otherwise leaving source size as max possible size
            // TODO : too much magic per line
            if (estimatedLengthChange < 0)
            {
                return default;
            }
            else if (estimatedLengthChange > MaxApproximationLength)
            {
                resultSize = MaxApproximationLength;
            }
            else if (estimatedLengthChange > 0 && resultSize < MaxApproximationLength)
            {
                // it is unusual that all the string is supposed to be replaced
                // TODO : too much magic per line
                decimal divisor = resultSize >= 100 || estimatedLengthChange >= 100 ? 25 : 5;
                resultSize += (int)Math.Ceiling((decimal)resultSize * (decimal)estimatedLengthChange / divisor);

                if (resultSize > MaxApproximationLength)
                {
                    resultSize = MaxApproximationLength;
                }
            }

            return str.ChangeTo(resultSize, call.Context.NewSource);
        }

        public class ReplaceStrArgs
        {
            public SqlStrTypeValue Searched { get; set; }

            public SqlStrTypeValue Replacement { get; set; }

            public SqlStrTypeValue SourceString { get; set; }
        }
    }
}
