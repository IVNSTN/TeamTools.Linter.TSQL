using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public abstract class BaseDateTimeFromParts : SqlGenericFunctionHandler<BaseDateTimeFromParts.DateTimeFromPartsArgs>
    {
        private static readonly SqlIntValueRange MonthRange = new SqlIntValueRange(1, 12);
        private static readonly SqlIntValueRange DayOfMonthRange = new SqlIntValueRange(1, 31);
        private static readonly SqlIntValueRange HourRange = new SqlIntValueRange(0, 24);
        private static readonly SqlIntValueRange MinuteRange = new SqlIntValueRange(0, 60);
        private static readonly SqlIntValueRange SecondsRange = new SqlIntValueRange(0, 60);
        private static readonly SqlIntValueRange FractionsRange = new SqlIntValueRange(0, 999);
        private static readonly SqlIntValueRange PrecisionRange = new SqlIntValueRange(0, 7);

        private readonly bool withDate;
        private readonly bool withTime;
        private readonly string outputType;

        protected BaseDateTimeFromParts(string funcName, int requiredArgumentCount, string outputType, bool withDate, bool withTime) : base(funcName, requiredArgumentCount)
        {
            this.withDate = withDate;
            this.withTime = withTime;
            this.outputType = outputType;
        }

        public override bool ValidateArgumentValues(CallSignature<DateTimeFromPartsArgs> call)
        {
            bool result = true;
            int argIdx = -1;

            if (withDate)
            {
                result &= ValidationScenario
                    .For("YEAR_VALUE", call.RawArgs[++argIdx], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.ValidatePositiveInt)
                    .Then(n => call.ValidatedArgs.Year = n)
                && ValidationScenario
                    .For("MONTH_VALUE", call.RawArgs[++argIdx], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, MonthRange))
                    .Then(n => call.ValidatedArgs.Month = n)
                && ValidationScenario
                    .For("DAY_VALUE", call.RawArgs[++argIdx], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, DayOfMonthRange))
                    .Then(n => call.ValidatedArgs.Day = n);
            }

            if (withTime)
            {
                result &= ValidationScenario
                    .For("HOUR_VALUE", call.RawArgs[++argIdx], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, HourRange))
                    .Then(n => call.ValidatedArgs.Hour = n)
                && ValidationScenario
                    .For("MINUTE_VALUE", call.RawArgs[++argIdx], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, MinuteRange))
                    .Then(n => call.ValidatedArgs.Minute = n);

                if (!string.Equals(outputType, "SMALLDATETIME", StringComparison.OrdinalIgnoreCase))
                {
                    // SMALLDATETIME does not have seconds and fractions
                    result &= ValidationScenario
                        .For("SECONDS_VALUE", call.RawArgs[++argIdx], call.Context)
                        .When(ArgumentIsValue.Validate)
                        .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, SecondsRange))
                        .Then(n => call.ValidatedArgs.Seconds = n)
                    && ValidationScenario
                        .For("FRACTIONS_VALUE", call.RawArgs[++argIdx], call.Context)
                        .When(ArgumentIsValue.Validate)
                        .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, FractionsRange))
                        .Then(n => call.ValidatedArgs.Fractions = n);
                }

                if (!withDate)
                {
                    // time-only version supports Precision
                    result &= ValidationScenario
                        .For("PRECISION_VALUE", call.RawArgs[++argIdx], call.Context)
                        .When(ArgumentIsValue.Validate)
                        .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.ValidateWithinRange(arg, val, success, PrecisionRange))
                        .Then(n => call.ValidatedArgs.Precision = n);
                }
            }

            return result;
        }

        protected override string DoEvaluateResultType(CallSignature<DateTimeFromPartsArgs> call) => outputType;

        protected override sealed SqlValue DoEvaluateResultValue(CallSignature<DateTimeFromPartsArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlDateTimeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            if (withDate)
            {
                if (call.ValidatedArgs.Year.IsNull
                || call.ValidatedArgs.Month.IsNull
                || call.ValidatedArgs.Day.IsNull)
                {
                    return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.NewSource.Node);
                }
            }

            if (withTime)
            {
                if (call.ValidatedArgs.Hour.IsNull
                || call.ValidatedArgs.Minute.IsNull
                || call.ValidatedArgs.Seconds.IsNull
                || call.ValidatedArgs.Fractions.IsNull
                || call.ValidatedArgs.Precision?.IsNull == true)
                {
                    return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.NewSource.Node);
                }
            }

            if (withDate)
            {
                if (!call.ValidatedArgs.Year.IsPreciseValue
                || !call.ValidatedArgs.Month.IsPreciseValue
                || !call.ValidatedArgs.Day.IsPreciseValue)
                {
                    // TODO : make approximation using ranges of arguments if provided
                    return value;
                }
            }

            if (withTime)
            {
                if (!call.ValidatedArgs.Hour.IsPreciseValue
                || !call.ValidatedArgs.Minute.IsPreciseValue
                || !call.ValidatedArgs.Seconds.IsPreciseValue
                || !call.ValidatedArgs.Fractions.IsPreciseValue)
                {
                    // TODO : make approximation using ranges of arguments if provided
                    return value;
                }
            }

            var year = call.ValidatedArgs.Year?.Value ?? 0;
            var month = call.ValidatedArgs.Month?.Value ?? 0;
            var day = call.ValidatedArgs.Day?.Value ?? 0;

            var hour = call.ValidatedArgs.Hour?.Value ?? 0;
            var min = call.ValidatedArgs.Minute?.Value ?? 0;
            var sec = call.ValidatedArgs.Seconds?.Value ?? 0;
            var ms = call.ValidatedArgs.Fractions?.Value ?? 0;

            return value.ChangeTo(new DateTime(year, month, day, hour, min, sec, ms), call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public sealed class DateTimeFromPartsArgs
        {
            public SqlIntTypeValue Year { get; set; }

            public SqlIntTypeValue Month { get; set; }

            public SqlIntTypeValue Day { get; set; }

            public SqlIntTypeValue Hour { get; set; }

            public SqlIntTypeValue Minute { get; set; }

            public SqlIntTypeValue Seconds { get; set; }

            public SqlIntTypeValue Fractions { get; set; }

            public SqlIntTypeValue Precision { get; set; }
        }
    }
}
