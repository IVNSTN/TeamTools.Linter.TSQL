using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DateDiff : SqlGenericFunctionHandler<DateDiff.DateDiffArgs>
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly string FuncName = "DATEDIFF";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;

        public DateDiff() : base(FuncName, RequiredArgumentCount)
        { }

        public override bool ValidateArgumentValues(CallSignature<DateDiffArgs> call)
        {
            return ValidationScenario
                .For("DATE_PART", call.RawArgs[0], call.Context)
                .When(ArgumentIsDatePart.Validate)
                .Then(d => call.ValidatedArgs.DatePart = d)
            && ValidationScenario
                .For("DATE_START", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.StartDate = d)
            && ValidationScenario
                .For("DATE_END", call.RawArgs[2], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.EndDate = d);
        }

        protected override string DoEvaluateResultType(CallSignature<DateDiffArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<DateDiffArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            var dateStart = call.ValidatedArgs.StartDate;
            var dateEnd = call.ValidatedArgs.EndDate;

            if (value is null || dateStart is null || dateEnd is null)
            {
                return value;
            }

            if (call.ValidatedArgs.StartDate.IsNull || call.ValidatedArgs.EndDate.IsNull)
            {
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.NewSource.Node);
            }

            if (!call.ValidatedArgs.StartDate.IsPreciseValue || !call.ValidatedArgs.EndDate.IsPreciseValue)
            {
                // TODO : If StartDate is from Past and EndDate is from Future
                // then the result can be limited [0 - +MaxInt].
                // If StartDate is from Future and EndDate is from Past
                // then the result can be limited [-MaxInt - 0].
                return value;
            }

            int dateDiff = 0;

            // TODO : support more date and time parts
            switch (call.ValidatedArgs.DatePart.DatePartValue)
            {
                case DatePartEnum.Year:
                    dateDiff = dateEnd.Value.Year - dateStart.Value.Year;
                    break;

                case DatePartEnum.Month:
                    dateDiff = (12 * (dateEnd.Value.Year - dateStart.Value.Year)) + (dateEnd.Value.Month - dateStart.Value.Month);
                    break;

                case DatePartEnum.Day:
                    dateDiff = (dateEnd.Value - dateStart.Value).Days;
                    break;

                default: return value;
            }

            return value.ChangeTo(dateDiff, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public sealed class DateDiffArgs
        {
            public DatePartArgument DatePart { get; set; }

            public SqlDateTimeValue StartDate { get; set; }

            public SqlDateTimeValue EndDate { get; set; }
        }
    }
}
