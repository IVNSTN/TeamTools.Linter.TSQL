using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class ConcatWs : SqlGenericFunctionHandler<ConcatWs.ConcatWsArgs>
    {
        private static readonly string FuncName = "CONCAT_WS";
        private static readonly int MinArgumentCount = 3;
        private static readonly int MaxArgumentCount = 254;

        public ConcatWs() : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ConcatWsArgs> call)
        {
            return ValidationScenario
                    .For("SEPARATOR", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.Separator = s)
                && AllArgsAreValues.Validate(
                    call.RawArgs.Skip(1).ToList(),
                    call.Context,
                    v => call.ValidatedArgs.Values.Add(v));
        }

        protected override string DoEvaluateResultType(CallSignature<ConcatWsArgs> call) => ConcatenationOutputTypeEvaluator.DoEvaluateResultType(call);

        // TODO : very similar to CONCAT implementation
        protected override SqlValue DoEvaluateResultValue(CallSignature<ConcatWsArgs> call)
        {
            if (call.ValidatedArgs.Separator.IsNull)
            {
                call.Context.RedundantCall("Separator is NULL");

                // TODO : refactoring needed
                return ((SqlStrTypeValue)call.Context.Converter
                    .ImplicitlyConvertTo(call.ResultType, base.DoEvaluateResultValue(call)))?
                    .ChangeTo("", call.Context.NewSource);
            }

            if (call.ValidatedArgs.Separator.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Separator is empty - consider using CONCAT");
            }

            SqlValue result = null;
            int i = 0;
            int n = call.ValidatedArgs.Values.Count;

            do
            {
                var str = (SqlStrTypeValue)call.Context.Converter.ImplicitlyConvertTo(call.ResultType, call.ValidatedArgs.Values[i]);
                if (str is null)
                {
                    // unable to convert value to string
                    result = null;
                }
                else if (result is null)
                {
                    // first assignment
                    result = str;
                }
                else if (str.IsNull || str.EstimatedSize == 0)
                {
                    // skipping this argument
                    // +2 because index is zero-based and the first arg in TSQL code is separator
                    call.Context.RedundantArgument($"{i + 2}", $"Argument {i + 2} is NULL or empty");
                }
                else
                {
                    result = str.TypeHandler.Sum(result, call.ValidatedArgs.Separator);
                    result = str.TypeHandler.Sum(result, str);
                }

                i++;
            }
            while (i < n && result != null);

            if ((result?.IsNull ?? false) && result is SqlStrTypeValue concatResult)
            {
                // CONCAT_WS returns empty string in case of null input
                return concatResult.ChangeTo("", call.Context.NewSource);
            }

            return result;
        }

        public sealed class ConcatWsArgs : ConcatenationArgs
        {
            public SqlStrTypeValue Separator { get; set; }
        }
    }
}
