using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    internal class DateAdd : SqlGenericFunctionHandler<DateAdd.DateAddArgs>
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly string FuncName = "DATEADD";
        private static readonly string FallbackOutputType = TSqlDomainAttributes.Types.DateTime;

        public DateAdd() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<DateAddArgs> call)
        {
            // we can still evaluate concrete result type even if increment is invalid
            ValidationScenario
                .For("DATE_INCREMENT", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidInt.Validate)
                .Then(n => call.ValidatedArgs.Number = n);

            return ValidationScenario
                .For("DATE_PART", call.RawArgs[0], call.Context)
                .When(ArgumentIsDatePart.Validate)
                .Then(s => call.ValidatedArgs.DatePart = s.DatePartValue)
            && ValidationScenario
                .For("DATE_VALUE", call.RawArgs[2], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(d => call.ValidatedArgs.DateValue = d);
        }

        protected override string DoEvaluateResultType(CallSignature<DateAddArgs> call)
        {
            if (call.ValidatedArgs.DateValue is SqlDateOnlyValue
            || call.ValidatedArgs.DateValue is SqlTimeOnlyValue
            || call.ValidatedArgs.DateValue is SqlDateTimeValue)
            {
                return call.ValidatedArgs.DateValue.TypeName;
            }

            return FallbackOutputType;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<DateAddArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlDateTimeValue>(base.DoEvaluateResultValue(call));
            var dateArg = call.Context.Converter.ImplicitlyConvert<SqlDateTimeValue>(call.ValidatedArgs.DateValue)?.Value;

            if (value is null || dateArg is null || call.ValidatedArgs.Number is null)
            {
                return value;
            }

            if (call.ValidatedArgs.Number.IsNull || call.ValidatedArgs.DateValue.IsNull)
            {
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.NewSource.Node);
            }

            if (!call.ValidatedArgs.Number.IsPreciseValue || !call.ValidatedArgs.DateValue.IsPreciseValue)
            {
                return value;
            }

            // we already ensured that both are not null
            var dateIncrement = call.ValidatedArgs.Number.Value;
            var dateEstimate = dateArg.Value;

            switch (call.ValidatedArgs.DatePart)
            {
                case DatePartEnum.Year:
                    dateEstimate = dateEstimate.AddYears(dateIncrement);
                    break;

                case DatePartEnum.Quarter:
                    dateEstimate = dateEstimate.AddMonths(dateIncrement * 3);
                    break;

                case DatePartEnum.Month:
                    dateEstimate = dateEstimate.AddMonths(dateIncrement);
                    break;

                case DatePartEnum.Day:
                    dateEstimate = dateEstimate.AddDays(dateIncrement);
                    break;

                case DatePartEnum.Hour:
                    dateEstimate = dateEstimate.AddHours(dateIncrement);
                    break;

                case DatePartEnum.Minute:
                    dateEstimate = dateEstimate.AddMinutes(dateIncrement);
                    break;

                case DatePartEnum.Second:
                    dateEstimate = dateEstimate.AddSeconds(dateIncrement);
                    break;

                case DatePartEnum.Millisecond:
                    dateEstimate = dateEstimate.AddMilliseconds(dateIncrement);
                    break;

                default: return value;
            }

            return value.ChangeTo(dateEstimate, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public sealed class DateAddArgs
        {
            public DatePartEnum DatePart { get; set; }

            public SqlIntTypeValue Number { get; set; }

            public SqlValue DateValue { get; set; }
        }
    }
}
