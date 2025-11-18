using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
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

        // TODO : validate date arg
        public override bool ValidateArgumentValues(CallSignature<DatePartArgs> call) => true;

        protected override string DoEvaluateResultType(CallSignature<DatePartArgs> call) => OutputType;

        // TODO : check if date value is NULL and return NULL
        protected override SqlValue DoEvaluateResultValue(CallSignature<DatePartArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            return value.ChangeTo(valueRange, call.Context.NewSource);
        }

        [ExcludeFromCodeCoverage]
        public class DatePartArgs
        {
            public ValueArgument DateValue { get; set; }
        }
    }
}
