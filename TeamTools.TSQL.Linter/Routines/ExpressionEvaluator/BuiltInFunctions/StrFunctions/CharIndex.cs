using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class CharIndex : SqlGenericFunctionHandler<CharIndex.CharIndexArgs>
    {
        private static readonly int MinArgCount = 2;
        private static readonly int MaxArgCount = 3;
        private static readonly string FuncName = "CHARINDEX";
        private static readonly string OutputType = "dbo.INT";
        private static readonly SqlIntValueRange PositiveInt = new SqlIntValueRange(1, int.MaxValue);

        public CharIndex() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<CharIndexArgs> call)
        {
            return ValidationScenario
                    .For("SUBSTR", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.SearchedSubstring = s)
                && ValidationScenario
                    .For("STR", call.RawArgs[1], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.SourceString = s)
                && (call.RawArgs.Count < 3
                  || ValidationScenario
                    .For("START", call.RawArgs[2], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.Validate(arg, val, success, PositiveInt))
                    .Then(i => call.ValidatedArgs.StartPos = i));
        }

        protected override string DoEvaluateResultType(CallSignature<CharIndexArgs> call) => OutputType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<CharIndexArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            if (call.ValidatedArgs.SearchedSubstring.IsNull || call.ValidatedArgs.SourceString.IsNull)
            {
                call.Context.RedundantCall("String and searched substring should not be NULL", call.ValidatedArgs.SearchedSubstring.Source);

                return value.TypeReference.MakeNullValue();
            }

            int startPos = 1;
            if (call.ValidatedArgs.StartPos != null)
            {
                if (call.ValidatedArgs.StartPos.EstimatedSize.High <= 0
                || call.ValidatedArgs.StartPos.IsNull)
                {
                    call.Context.RedundantCall("Start position should be 1 or higher");
                }
                else if (call.ValidatedArgs.StartPos.IsPreciseValue && call.ValidatedArgs.StartPos.Value > 1)
                {
                    startPos = call.ValidatedArgs.StartPos.Value;
                }
            }

            // TODO : Respect DB case sensitivity. Maybe put an option to the linter config.
            if (call.ValidatedArgs.SearchedSubstring.IsPreciseValue
            && call.ValidatedArgs.SourceString.IsPreciseValue
            && (call.ValidatedArgs.StartPos?.IsPreciseValue ?? true))
            {
                int idx = call.ValidatedArgs.SourceString.Value.IndexOf(call.ValidatedArgs.SearchedSubstring.Value, startPos - 1) + 1;
                return value.ChangeTo(idx, call.ValidatedArgs.SearchedSubstring.Source);
            }

            if (call.ValidatedArgs.SearchedSubstring.IsPreciseValue
            && call.ValidatedArgs.SearchedSubstring.EstimatedSize >= call.ValidatedArgs.SourceString.EstimatedSize)
            {
                // searched substring is longer than the string
                return value.ChangeTo(0, call.ValidatedArgs.SearchedSubstring.Source);
            }

            return value.ChangeTo(new SqlIntValueRange(0, call.ValidatedArgs.SourceString.EstimatedSize), call.ValidatedArgs.SourceString.Source);
        }

        public class CharIndexArgs
        {
            public SqlStrTypeValue SearchedSubstring { get; set; }

            public SqlStrTypeValue SourceString { get; set; }

            public SqlIntTypeValue StartPos { get; set; }
        }
    }
}
