using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    public class Rand : SqlGenericFunctionHandler<Rand.RandArgs>
    {
        private static readonly int MinArgCount = 0;
        private static readonly int MaxArgCount = 1;
        private static readonly string FuncName = "RAND";
        private static readonly string ResultType = TSqlDomainAttributes.Types.Int; // TODO : FLOAT
        private static readonly SqlIntValueRange RandRange = new SqlIntValueRange(0, 1);

        public Rand() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<RandArgs> call)
        {
            return call.RawArgs.Count == 0
                || ValidationScenario
                    .For("SEED", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
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
