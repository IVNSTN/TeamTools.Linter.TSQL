using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class EndOfMonth : SqlGenericFunctionHandler<EndOfMonth.EndOfMonthArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly string FuncName = "EOMONTH";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Date;

        public EndOfMonth() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<EndOfMonthArgs> call)
        {
            return ValidationScenario
                .For("DATE_VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.DateValue = d);
        }

        protected override string DoEvaluateResultType(CallSignature<EndOfMonthArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<EndOfMonthArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlDateOnlyValue>(base.DoEvaluateResultValue(call));
            if (value is null || call.ValidatedArgs.DateValue?.IsPreciseValue != true)
            {
                return value;
            }

            if (call.ValidatedArgs.DateValue.IsNull)
            {
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.NewSource.Node);
            }

            var newDate = GetLastDayOfMonth(call.ValidatedArgs.DateValue.Value);

            return value.ChangeTo(newDate, call.Context.NewSource);
        }

        private static DateTime GetLastDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        [ExcludeFromCodeCoverage]
        public sealed class EndOfMonthArgs
        {
            public SqlDateTimeValue DateValue { get; set; }
        }
    }
}
