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

        public override bool ValidateArgumentValues(CallSignature<DatePartArgs> call)
        {
            // Even if date is invalid we can still estimate result range based on DATEPART value
            ValidationScenario
                .For("DATE_VALUE", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.DateValue = d);

            return ValidationScenario
                .For("DATE_PART", call.RawArgs[0], call.Context)
                .When(ArgumentIsDatePart.Validate)
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

            // TODO : if src is date only then attempt to extract time smells bad
            // TODO : if src is time then attempt to extract date from it smells bad
            var dt = call.ValidatedArgs.DateValue;
            if (dt?.IsPreciseValue == true
            && DatePartExtractor.ExtractDatePartFromSpecificDate(dt.Value, call.ValidatedArgs.DatePart.DatePartValue, out int datePartValue))
            {
                // precise estimate
                return value.ChangeTo(datePartValue, call.Context.NewSource);
            }

            return value.ChangeTo(datePartEstimate, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DatePartArgs
        {
            public DatePartArgument DatePart { get; set; }

            public SqlDateTimeValue DateValue { get; set; }
        }
    }
}
