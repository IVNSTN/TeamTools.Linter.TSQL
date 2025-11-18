using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    // TODO : support generic numeric type
    public class Sign : SqlGenericFunctionHandler<Sign.SignArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly string FuncName = "SIGN";
        private static readonly string FallbackResultType = TSqlDomainAttributes.Types.Int;
        private static readonly SqlIntValueRange SignRange = new SqlIntValueRange(-1, 1);

        public Sign() : base(FuncName, RequiredArgumentCount)
        { }

        public override bool ValidateArgumentValues(CallSignature<SignArgs> call)
        {
            ValidationScenario
                .For("NUMBER", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                // .And(ArgumentIsValidInt.Validate)
                .Then(v => call.ValidatedArgs.Value = v);

            // Even if source value is unknown the result range is still pretty narrow
            return true;
        }

        protected override string DoEvaluateResultType(CallSignature<SignArgs> call)
            => call.ValidatedArgs.Value?.TypeName ?? FallbackResultType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<SignArgs> call)
        {
            var resultRange = call.Context.Converter
                .ImplicitlyConvert<SqlIntTypeValue>(call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node))?
                .ChangeTo(SignRange, call.Context.NewSource);

            if (call.ValidatedArgs.Value is null || resultRange is null)
            {
                return resultRange;
            }

            if (call.ValidatedArgs.Value.IsNull)
            {
                call.Context.RedundantCall("Value is NULL");
                return resultRange.TypeReference.MakeNullValue();
            }

            var result = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(call.ValidatedArgs.Value);

            if (result is null)
            {
                // unsupported type
                return resultRange;
            }

            if (call.ValidatedArgs.Value.IsPreciseValue)
            {
                call.Context.RedundantCall("Value is precise, sign is well known at compile time");
            }

            // TODO : somehow move to abstractions
            if (call.ValidatedArgs.Value is SqlIntTypeValue intVal)
            {
                if (intVal.IsPreciseValue)
                {
                    return result.ChangeTo(System.Math.Sign(intVal.Value), call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.High < 0)
                {
                    return result.ChangeTo(-1, call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.High == 0)
                {
                    return result.ChangeTo(new SqlIntValueRange(-1, 0), call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.Low > 0)
                {
                    return result.ChangeTo(1, call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.Low == 0)
                {
                    return result.ChangeTo(new SqlIntValueRange(0, 1), call.Context.NewSource);
                }
            }

            return resultRange;
        }

        public class SignArgs
        {
            public SqlValue Value { get; set; }
        }
    }
}
