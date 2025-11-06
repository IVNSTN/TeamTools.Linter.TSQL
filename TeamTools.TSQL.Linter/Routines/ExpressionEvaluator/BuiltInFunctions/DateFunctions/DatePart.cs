using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DatePart : SqlGenericFunctionHandler<DatePart.DatePartArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string FuncName = "DATEPART";
        private static readonly string OutputType = "dbo.INT";
        private static readonly SqlIntValueRange MaxDatePartRange = new SqlIntValueRange(0, int.MaxValue);

        public DatePart() : base(FuncName, RequiredArgumentCount)
        {
        }

        // TODO : validate date arg
        public override bool ValidateArgumentValues(CallSignature<DatePartArgs> call)
        {
            return ValidationScenario
                .For("DATEPART", call.RawArgs[0], call.Context)
                .When<DatePartArgument>(ArgumentIsDatePart.Validate)
                .Then(d => call.ValidatedArgs.DatePart = d);
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

            public ValueArgument DateValue { get; set; }
        }
    }
}
