using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DateName : SqlGenericFunctionHandler<DateName.DateNameArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string FuncName = "DATENAME";
        private static readonly string OutputType = TSqlDomainAttributes.Types.NVarchar;

        public DateName() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<DateNameArgs> call)
        {
            // we can still estimate result value range even if date value is unknown
            ValidationScenario
                .For("DATE_VALUE", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.DateValue = d);

            return ValidationScenario
                .For("DATEPART", call.RawArgs[0], call.Context)
                .When(ArgumentIsDatePart.Validate)
                .Then(d => call.ValidatedArgs.DatePart = d);
        }

        protected override string DoEvaluateResultType(CallSignature<DateNameArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<DateNameArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            var lengthEstimate = DatePartToRangeConverter.GetDateNameLengthEstimate(call.ValidatedArgs.DatePart.DatePartValue);
            if (lengthEstimate <= 0)
            {
                // unknown or unsupported date part
                return value;
            }

            // Extracting concrete date part from provided precise date/time value
            var dateValue = call.ValidatedArgs.DateValue;
            if (dateValue?.IsPreciseValue == true
            && DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue.Value, call.ValidatedArgs.DatePart.DatePartValue, out int datePartValue))
            {
                switch (call.ValidatedArgs.DatePart.DatePartValue)
                {
                    // These two are culture-specific. Others are just numbers.
                    case DatePartEnum.Month:
                    case DatePartEnum.DayOfWeek:
                        break;
                    default: return value.ChangeTo(datePartValue.ToString(), call.Context.NewSource);
                }
            }

            return value.ChangeTo(lengthEstimate, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DateNameArgs
        {
            public DatePartArgument DatePart { get; set; }

            public SqlDateTimeValue DateValue { get; set; }
        }
    }
}
