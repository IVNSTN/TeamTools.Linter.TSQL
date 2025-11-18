using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Len : SqlGenericFunctionHandler<Len.LenArgs>
    {
        private static readonly int RequiredArgCount = 1;
        private static readonly string FuncName = "LEN";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;

        public Len() : base(FuncName, RequiredArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<LenArgs> call)
        {
            return ValidationScenario
                .For("STR", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.Str = s);
        }

        protected override string DoEvaluateResultType(CallSignature<LenArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<LenArgs> call)
        {
            var value = call.Context.Converter
                .ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            if (call.ValidatedArgs.Str.IsNull)
            {
                call.Context.RedundantCall("STR is NULL");
                return value.TypeHandler.IntValueFactory.MakeNull(call.Context.Node);
            }

            // TODO : If type is variable should be provided from the type reference
            if (call.ValidatedArgs.Str.IsPreciseValue)
            {
                if (call.ValidatedArgs.Str.TypeName.StartsWith("VAR"))
                {
                    return value.ChangeTo(call.ValidatedArgs.Str.Value.TrimEnd().Length, call.Context.NewSource);
                }

                return value.ChangeTo(call.ValidatedArgs.Str.Value.Length, call.Context.NewSource);
            }

            return value.ChangeTo(new SqlIntValueRange(0, call.ValidatedArgs.Str.EstimatedSize), call.Context.NewSource);
        }

        public class LenArgs
        {
            public SqlStrTypeValue Str { get; set; }
        }
    }
}
