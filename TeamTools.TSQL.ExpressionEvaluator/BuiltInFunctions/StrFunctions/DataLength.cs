using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class DataLength : SqlGenericFunctionHandler<DataLength.DataLengthArgs>
    {
        private static readonly int RequiredArgCount = 1;
        private static readonly string FuncName = "DATALENGTH";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;

        public DataLength() : base(FuncName, RequiredArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<DataLengthArgs> call)
        {
            return ValidationScenario
                .For("STR", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.Str = s);
        }

        protected override string DoEvaluateResultType(CallSignature<DataLengthArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<DataLengthArgs> call)
        {
            var value = call.Context.Converter
                .ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            if (call.ValidatedArgs.Str.IsNull)
            {
                call.Context.RedundantCall("STR is NULL");
                return value.TypeHandler.IntValueFactory.MakeNull(call.Context.Node);
            }

            // TODO : get it from TypeReference property or something like that
            if (call.ValidatedArgs.Str.TypeName.IndexOf("VAR", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                call.Context.RedundantCall("STR has fixed length");
            }

            // TODO : get rid of magic cast
            int bytes = (call.ValidatedArgs.Str.TypeReference as SqlStrTypeReference).Bytes;

            if (call.ValidatedArgs.Str.IsPreciseValue)
            {
                return value.ChangeTo(bytes, call.Context.NewSource);
            }

            return value.ChangeTo(new SqlIntValueRange(0, bytes), call.Context.NewSource);
        }

        public class DataLengthArgs
        {
            public SqlStrTypeValue Str { get; set; }
        }
    }
}
