using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    public class Rand : SqlGenericFunctionHandler<Rand.RandArgs>
    {
        private static readonly int MinArgCount = 0;
        private static readonly int MaxArgCount = 1;
        private static readonly string FuncName = "RAND";
        private static readonly string ResultType = "dbo.INT"; // TODO : FLOAT
        private static readonly SqlIntValueRange RandRange = new SqlIntValueRange(0, 1);

        public Rand() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<RandArgs> call)
        {
            return call.RawArgs.Count == 0
                || ValidationScenario
                    .For("SEED", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, src, success) => ArgumentIsValidInt.Validate(arg, src, success))
                    .Then(i => call.ValidatedArgs.Seed = i);
        }

        protected override string DoEvaluateResultType(CallSignature<RandArgs> call) => ResultType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<RandArgs> call)
        {
            return call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call))?
                .ChangeTo(RandRange, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class RandArgs
        {
            public SqlIntTypeValue Seed { get; set; }
        }
    }
}
