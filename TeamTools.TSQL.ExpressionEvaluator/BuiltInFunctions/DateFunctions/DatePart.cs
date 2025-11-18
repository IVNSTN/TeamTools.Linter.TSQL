using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DatePart : SqlGenericFunctionHandler<DatePart.DatePartArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string FuncName = "DATEPART";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;
        private static readonly SqlIntValueRange MaxDatePartRange = new SqlIntValueRange(0, int.MaxValue);

        public DatePart() : base(FuncName, RequiredArgumentCount)
        {
        }

        // TODO : validate date arg
        public override bool ValidateArgumentValues(CallSignature<DatePartArgs> call)
        {
            return ValidationScenario
                .For("DATEPART", call.RawArgs[0], call.Context)
                .When(ArgumentIsDatePart.Validate)
                .Then(d => call.ValidatedArgs.DatePart = d)
            && ValidationScenario
                .For("DATE", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(s => call.ValidatedArgs.DateValue = s);
        }

        protected override string DoEvaluateResultType(CallSignature<DatePartArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<DatePartArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            var datePartEstimate = DatePartToRangeConverter.GetDatePartRange(call.ValidatedArgs.DatePart.DatePartValue);
            if (datePartEstimate is null)
            {
                // unknown or unsupported
                return value.ChangeTo(MaxDatePartRange, call.Context.NewSource);
            }

            return value.ChangeTo(datePartEstimate, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DatePartArgs
        {
            public DatePartArgument DatePart { get; set; }

            public SqlValue DateValue { get; set; }
        }
    }
}
