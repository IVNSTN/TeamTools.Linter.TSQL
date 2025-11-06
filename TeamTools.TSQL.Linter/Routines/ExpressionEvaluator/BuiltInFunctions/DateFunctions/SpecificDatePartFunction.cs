using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public abstract class SpecificDatePartFunction : SqlGenericFunctionHandler<SpecificDatePartFunction.DatePartArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly string OutputType = "dbo.INT";
        private readonly SqlIntValueRange valueRange;

        public SpecificDatePartFunction(string funcName, SqlIntValueRange valueRange) : base(funcName, RequiredArgumentCount)
        {
            this.valueRange = valueRange;
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
