using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Concat : SqlGenericFunctionHandler<Concat.ConcatArgs>
    {
        private static readonly string FuncName = "CONCAT";
        private static readonly int MinArgumentCount = 2;
        private static readonly int MaxArgumentCount = 254;

        public Concat() : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ConcatArgs> call)
        {
            return AllArgsAreValues.Validate(call.RawArgs, call.Context, v => call.ValidatedArgs.Values.Add(v));
        }

        protected override string DoEvaluateResultType(CallSignature<ConcatArgs> call) => ConcatenationOutputTypeEvaluator.DoEvaluateResultType(call);

        protected override SqlValue DoEvaluateResultValue(CallSignature<ConcatArgs> call)
        {
            int i = 0;
            int n = call.ValidatedArgs.Values.Count;
            SqlValue result = null;

            do
            {
                // TODO : refactoring needed
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
                    call.Context.RedundantArgument($"{i + 1}", $"Argument {i + 1} is NULL or empty");
                    // and skipping
                }
                else
                {
                    result = str.TypeHandler.Sum(result, str);
                }

                i++;
            }
            while (i < n && result != null);

            if ((result?.IsNull ?? false) && result is SqlStrTypeValue concatResult)
            {
                // CONCAT returns empty string in case of null input
                return concatResult.ChangeTo("", call.Context.NewSource);
            }

            return result;
        }

        public sealed class ConcatArgs : ConcatenationArgs
        { }
    }
}
