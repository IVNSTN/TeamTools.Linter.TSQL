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

        // TODO : validate date arg
        public override bool ValidateArgumentValues(CallSignature<DateNameArgs> call)
        {
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

            return value.ChangeTo(lengthEstimate, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DateNameArgs
        {
            public DatePartArgument DatePart { get; set; }

            public ValueArgument DateValue { get; set; }
        }
    }
}
