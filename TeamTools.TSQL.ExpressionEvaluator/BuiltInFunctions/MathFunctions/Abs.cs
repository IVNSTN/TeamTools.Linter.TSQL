using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    // TODO : support generic numeric type
    public class Abs : SqlGenericFunctionHandler<Abs.AbsArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly string FuncName = "ABS";
        private static readonly SqlIntValueRange PositiveInt = new SqlIntValueRange(0, int.MaxValue);

        public Abs() : base(FuncName, RequiredArgumentCount)
        { }

        public override bool ValidateArgumentValues(CallSignature<AbsArgs> call)
        {
            return ValidationScenario
                .For("NUMBER", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                // .And(ArgumentIsValidInt.Validate)
                .Then(i => call.ValidatedArgs.Value = i);
        }

        protected override string DoEvaluateResultType(CallSignature<AbsArgs> call)
            => call.ValidatedArgs.Value.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<AbsArgs> call)
        {
            if (call.ValidatedArgs.Value.IsNull)
            {
                call.Context.RedundantCall("Value is NULL");
                return call.ValidatedArgs.Value.TypeReference.MakeNullValue();
            }

            var result = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(call.ValidatedArgs.Value)?
                .ChangeTo(PositiveInt, call.Context.NewSource);

            if (result is null)
            {
                // unsupported type
                return default;
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
                    return result.ChangeTo(System.Math.Abs(intVal.Value), call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.High <= 0)
                {
                    return result.ChangeTo(SqlIntValueRange.RevertRange(intVal.EstimatedSize), call.Context.NewSource);
                }
                else if (intVal.EstimatedSize.Low >= 0)
                {
                    call.Context.RedundantCall("Value is not negative");
                    return intVal;
                }
            }

            return result;
        }

        public class AbsArgs
        {
            public SqlValue Value { get; set; }
        }
    }
}
