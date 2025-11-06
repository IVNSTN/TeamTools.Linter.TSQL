using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    // TODO : This is very similar to OriginalStringManipulation
    public abstract class TrimmingFunction : SqlGenericFunctionHandler<TrimmingFunction.TrimStrArgs>
    {
        private static readonly int MaxReadableLiteral = 32;
        private static readonly int MinArgCount = 1;
        private static readonly int MaxArgCount = 2;
        private readonly string redundantModificationDescr;

        public TrimmingFunction(string funcName, string redundantModificationDescr)
        : base(funcName, MinArgCount, MaxArgCount)
        {
            this.redundantModificationDescr = redundantModificationDescr;
        }

        public override bool ValidateArgumentValues(CallSignature<TrimStrArgs> call)
        {
            return ValidationScenario
                    .For("STR", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.SourceString = s)
                && (call.RawArgs.Count < 2
                || ValidationScenario
                    .For("TRIMMED_CHARS", call.RawArgs[1], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.TrimmedChars = s));
        }

        protected override string DoEvaluateResultType(CallSignature<TrimStrArgs> call)
            => call.ValidatedArgs.SourceString.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<TrimStrArgs> call)
        {
            var str = call.ValidatedArgs.SourceString;
            var trimmedChars = call.ValidatedArgs.TrimmedChars;

            if (str.IsNull)
            {
                call.Context.RedundantCall("Source string is NULL");
                return str.TypeReference.MakeNullValue();
            }

            if (str.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Source string is empty");
                return str;
            }

            if (trimmedChars?.IsNull ?? false)
            {
                call.Context.RedundantCall("Trimmed char is NULL");
                return str.TypeReference.MakeNullValue();
            }

            if (trimmedChars != null && trimmedChars.IsPreciseValue
            && trimmedChars.Value.Equals(" "))
            {
                call.Context.RedundantArgument("TRIMMED_CHAR", "Space is the default trimmed char");
            }

            if (FunctionName == "RTRIM" && str.Source is SqlValueSourceVariable varSrc
            && str.TypeName.IndexOf("VAR", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                call.Context.RedundantCall($"Consider using VARCHAR type for {varSrc.VarName} instead of trimming");
            }

            if (!str.IsPreciseValue)
            {
                return str;
            }

            if (trimmedChars != null && !trimmedChars.IsPreciseValue)
            {
                return str.TypeReference.MakeUnknownValue();
            }

            string modifiedString = PerformTrimming(str.Value, trimmedChars?.Value ?? " ");

            if (string.Equals(modifiedString, str.Value))
            {
                string msg = redundantModificationDescr;
                if (str.Value.Length <= MaxReadableLiteral)
                {
                    msg = $"'{str.Value}' {redundantModificationDescr}";
                }

                call.Context.RedundantCall(msg);
            }

            return str.ChangeTo(modifiedString, call.Context.NewSource);
        }

        protected abstract string ModifyString(string originalValue, char[] trimmedChar);

        private string PerformTrimming(string str, string trimmedChars)
        {
            if (string.IsNullOrEmpty(trimmedChars))
            {
                trimmedChars = " ";
            }

            var trimCharList = trimmedChars.ToCharArray();

            return ModifyString(str, trimCharList);
        }

        public class TrimStrArgs
        {
            public SqlStrTypeValue SourceString { get; set; }

            public SqlStrTypeValue TrimmedChars { get; set; }
        }
    }
}
