using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class OriginalStringPartHandler : SqlGenericFunctionHandler<OriginalStringPartHandler.StrPartArgs>
    {
        private static readonly int HalfOfMaxInt = int.MaxValue / 2;
        private static readonly int MaxReadableLength = 24;
        private static readonly string FallbackResultType = TSqlDomainAttributes.Types.Varchar;

        protected OriginalStringPartHandler(string funcName, int requiredArgs) : base(funcName, requiredArgs)
        {
        }

        protected OriginalStringPartHandler(string funcName, int minArgs, int maxArgs) : base(funcName, minArgs, maxArgs)
        {
        }

        public SqlStrTypeValue Evaluate(SqlStrTypeValue str, SqlIntTypeValue start, SqlIntTypeValue len, EvaluationContext context)
        {
            if (str.IsNull)
            {
                context.RedundantCall("Source string is NULL", str.Source);
                return str.TypeHandler.StrValueFactory.MakeNull(context.Node);
            }

            if (len.IsNull || (start?.IsNull ?? false))
            {
                context.RedundantCall("Length or start is NULL", str.Source);
                return str.TypeHandler.StrValueFactory.MakeNull(context.Node);
            }

            if (str.EstimatedSize == 0)
            {
                context.RedundantCall("Source string is empty", str.Source);
                return str.TypeHandler.StrValueFactory.MakePreciseValue(str.TypeName, "", context.NewSource);
            }

            // taking longest substring size
            int lenValue = len.IsPreciseValue ? len.Value : len.EstimatedSize.High;

            // taking minimum start position
            int startValue = start?.IsPreciseValue ?? false ? start.Value : start?.EstimatedSize.Low ?? 0;

            // TODO : actually SUBSTRING in TSQL supports negative start and evaluates
            // result for start = -1 and length = 3 as first char of the source string
            if (startValue <= 0 && start?.EstimatedSize.High > 0)
            {
                // unknown INT low bound is negative but we may assume that actually
                // intent was to provide something zero-based
                startValue = 1;
            }

            if (lenValue == 0 || startValue > str.EstimatedSize || lenValue >= HalfOfMaxInt)
            {
                if ((start?.EstimatedSize.Low ?? startValue) >= HalfOfMaxInt
                || (start?.EstimatedSize.Low ?? startValue) <= -HalfOfMaxInt
                || (start?.EstimatedSize.High ?? startValue) >= HalfOfMaxInt
                || lenValue >= HalfOfMaxInt)
                {
                    // something unpredictable
                    // but it cannot be longer the original value
                    // however estimating to something would lead to many false-positive
                    // violation detections
                    // return str.ChangeTo(str.EstimatedSize, context.NewSource);
                    return default;
                }

                // TODO : RedundantCall?
                return str.ChangeTo("", context.NewSource);
            }

            int usedLenValue = lenValue;
            if (startValue + lenValue > str.EstimatedSize)
            {
                usedLenValue = str.EstimatedSize - startValue + 1;
            }

            if (!str.IsPreciseValue)
            {
                return str.ChangeTo(usedLenValue, context.NewSource);
            }

            // precise
            if (str.Value.Length <= lenValue && startValue == 0)
            {
                string msg = str.EstimatedSize.ToString();

                if (str.EstimatedSize < MaxReadableLength)
                {
                    msg = $"'{str.Value}'";
                }
                else if (str.Source is SqlValueSourceVariable varSrc)
                {
                    msg = varSrc.VarName;
                }

                if (str.EstimatedSize == lenValue)
                {
                    msg = $"{msg} is already of that size";
                }
                else
                {
                    msg = $"{msg} is already shorter than {lenValue}";
                }

                context.RedundantCall(msg, str.Source);

                return str.ChangeTo(str.Value, context.NewSource);
            }

            var modifiedString = TakeStringPartFrom(str.Value, startValue, usedLenValue);

            if (str.SourceKind != SqlValueSourceKind.Expression
            && string.Equals(modifiedString, str.Value))
            {
                context.RedundantCall($"'{str.Value}' is already of that size", str.Source);
            }

            return str.ChangeTo(modifiedString, context.NewSource);
        }

        public override bool ValidateArgumentValues(CallSignature<StrPartArgs> call)
        {
            int startParam = call.RawArgs.Count < 3 ? -1 : 1;
            int lengthParam = call.RawArgs.Count < 3 ? 1 : 2;

            return (ValidationScenario
                    .For("STR", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.Text = s)
                    // if we could not parse source but parsed length
                    // then we can estimate to approximate string size
                    || true)
                && ValidationScenario
                    .For("LENGTH", call.RawArgs[lengthParam], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.ValidatePositiveInt)
                    .Then(i => call.ValidatedArgs.Length = i)
                && (startParam < 0
                || ValidationScenario
                    .For("START", call.RawArgs[startParam], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.ValidatePositiveInt)
                    .Then(i => call.ValidatedArgs.Start = i));
        }

        protected override string DoEvaluateResultType(CallSignature<StrPartArgs> call)
            => call.ValidatedArgs.Text?.TypeName ?? FallbackResultType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<StrPartArgs> call)
        {
            if (call.ValidatedArgs.Text is null)
            {
                int resultLength = call.ValidatedArgs.Length.EstimatedSize.High;
                if (resultLength == int.MaxValue)
                {
                    // something absolutely unpredictable
                    return default;
                }

                return call.Context.Converter
                    .ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call))?
                    .ChangeTo(resultLength, call.Context.NewSource);
            }

            return Evaluate(
                call.ValidatedArgs.Text,
                call.ValidatedArgs.Start,
                call.ValidatedArgs.Length,
                call.Context);
        }

        protected abstract string TakeStringPartFrom(string srcValue, int startValue, int lenValue);

        public class StrPartArgs
        {
            public SqlStrTypeValue Text { get; set; }

            public SqlIntTypeValue Start { get; set; }

            public SqlIntTypeValue Length { get; set; }
        }
    }
}
