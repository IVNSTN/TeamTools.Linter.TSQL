using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public abstract class SpecificDatePartFunction : SqlGenericFunctionHandler<SpecificDatePartFunction.DatePartArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;
        private readonly SqlIntValueRange valueRange;
        private readonly DatePartEnum datePart;

        protected SpecificDatePartFunction(string funcName, SqlIntValueRange valueRange, DatePartEnum datePart) : base(funcName, RequiredArgumentCount)
        {
            this.valueRange = valueRange;
            this.datePart = datePart;
        }

        public override bool ValidateArgumentValues(CallSignature<DatePartArgs> call)
        {
            ValidationScenario
                .For("DATE_VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidDateTime.Validate)
                .Then(d => call.ValidatedArgs.DateValue = d);

            // we still can estimate result range no matter if the date source is unclear
            return true;
        }

        protected override string DoEvaluateResultType(CallSignature<DatePartArgs> call) => OutputType;

        // TODO : check if date value is NULL and return NULL
        protected override SqlValue DoEvaluateResultValue(CallSignature<DatePartArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            var dt = call.ValidatedArgs.DateValue;
            if (dt?.IsPreciseValue == true
            && DatePartExtractor.ExtractDatePartFromSpecificDate(dt.Value, datePart, out int datePartValue))
            {
                // precise estimate
                return value.ChangeTo(datePartValue, call.Context.NewSource);
            }

            // Estimated result range based on provided DatePart value
            return value.ChangeTo(valueRange, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DatePartArgs
        {
            public SqlDateTimeValue DateValue { get; set; }
        }
    }
}
