using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class DecodeSymbolFromNumbers : SqlGenericFunctionHandler<DecodeSymbolFromNumbers.CharCodeArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private readonly string outputType;

        public DecodeSymbolFromNumbers(string funcName, string outputType)
        : base(funcName, RequiredArgumentCount)
        {
            this.outputType = outputType;
        }

        public override bool ValidateArgumentValues(CallSignature<CharCodeArgs> call)
        {
            ValidationScenario
                .For("CHAR_CODE", call.RawArgs[0], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .And<SqlIntTypeValue>((arg, src, success) =>
                    ArgumentIsValidInt.Validate(arg, src, success))
                .Then(i => call.ValidatedArgs.CharCode = i);

            // if char code could not be parsed we still can estimate
            // to approximate value
            return true;
        }

        protected override string DoEvaluateResultType(CallSignature<CharCodeArgs> call) => outputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<CharCodeArgs> call)
        {
            var str = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));

            if (str is null)
            {
                return default;
            }

            if (call.ValidatedArgs.CharCode is null || !call.ValidatedArgs.CharCode.IsPreciseValue)
            {
                return str;
            }

            if (call.ValidatedArgs.CharCode.IsNull)
            {
                call.Context.RedundantCall("CHAR_CODE is NULL");

                return str.TypeHandler.StrValueFactory.MakeNull(call.Context.Node);
            }

            var range = GetValidRange();
            if (range != null && !range.IsValueWithin(call.ValidatedArgs.CharCode.Value))
            {
                call.Context.InvalidArgument("CHAR_CODE", "is out of supported range");

                return str.TypeHandler.StrValueFactory.MakeNull(call.Context.Node);
            }

            try
            {
                var decodedStr = char.ConvertFromUtf32(call.ValidatedArgs.CharCode.Value);

                return str.ChangeTo(decodedStr, call.Context.NewSource);
            }
            catch
            {
                return str;
            }
        }

        protected virtual SqlIntValueRange GetValidRange() => default;

        public class CharCodeArgs
        {
            public SqlIntTypeValue CharCode { get; set; }
        }
    }
}
