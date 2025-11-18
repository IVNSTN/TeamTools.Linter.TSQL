using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class EncodeSymbolsAsNumbers : SqlGenericFunctionHandler<EncodeSymbolsAsNumbers.CharValueArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private readonly string outputType;
        private readonly SqlIntValueRange outputRange;

        protected EncodeSymbolsAsNumbers(string funcName, string outputType, SqlIntValueRange outputRange = null) : base(funcName, RequiredArgumentCount)
        {
            this.outputType = outputType;
            this.outputRange = outputRange;
        }

        public override bool ValidateArgumentValues(CallSignature<CharValueArgs> call)
        {
            ValidationScenario
                .For("CHAR_VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.SymbolValue = s);

            // if char value could not be parsed we still can estimate
            // to approximate value
            return true;
        }

        protected override string DoEvaluateResultType(CallSignature<CharValueArgs> call) => outputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<CharValueArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }
            else if (outputRange != null)
            {
                value = value.ChangeTo(outputRange, call.Context.NewSource);
            }

            if (call.ValidatedArgs.SymbolValue is null)
            {
                // could not evaluate argument but output type is predefined
                return value;
            }

            if (call.ValidatedArgs.SymbolValue.IsNull
            || call.ValidatedArgs.SymbolValue.EstimatedSize == 0)
            {
                // TODO : reuse argument name defined in validation stage
                call.Context.RedundantCall("CHAR_VALUE is NULL or empty", call.ValidatedArgs.SymbolValue.Source);
                return value.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.SymbolValue.EstimatedSize > 1)
            {
                var str = call.ValidatedArgs.SymbolValue.IsPreciseValue ? call.ValidatedArgs.SymbolValue.Value : null;

                // ASCII and UNICODE work with the first symbol only
                call.Context.ImplicitTruncation(1, call.ValidatedArgs.SymbolValue.EstimatedSize, str);
            }

            if (call.ValidatedArgs.SymbolValue.IsPreciseValue)
            {
                return ClarifySymbolCodeValue(call.ValidatedArgs.SymbolValue, value, call.Context);
            }

            return value;
        }

        protected abstract SqlIntTypeValue ClarifySymbolCodeValue(SqlStrTypeValue str, SqlIntTypeValue res, EvaluationContext context);

        public class CharValueArgs
        {
            public SqlStrTypeValue SymbolValue { get; set; }
        }
    }
}
