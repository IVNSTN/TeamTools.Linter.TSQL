using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    // TODO : support generic numeric type
    public class Sign : SqlGenericFunctionHandler<Sign.SignArgs>
    {
        private static readonly int RequiredArgs = 1;
        private static readonly string FuncName = "SIGN";
        private static readonly string FallbackResultType = "dbo.INT";
        private static readonly SqlIntValueRange SignRange = new SqlIntValueRange(-1, 1);

        public Sign() : base(FuncName, RequiredArgs)
        { }

        public override bool ValidateArgumentValues(CallSignature<SignArgs> call)
        {
            ValidationScenario
                .For("NUMBER", call.RawArgs[0], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                // .And<SqlIntTypeValue>((arg, src, success) => ArgumentIsValidInt.Validate(arg, src, success))
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
