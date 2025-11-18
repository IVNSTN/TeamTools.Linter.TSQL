using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class OriginalStringManipulation : SqlGenericFunctionHandler<OriginalStringManipulation.ModifyStrArgs>
    {
        private static readonly int MaxReadableLiteral = 32;
        private static readonly int RequiredArgumentCount = 1;
        private readonly string redundantModificationDescr;

        protected OriginalStringManipulation(string funcName, string redundantModificationDescr)
        : base(funcName, RequiredArgumentCount)
        {
            this.redundantModificationDescr = redundantModificationDescr;
        }

        public override bool ValidateArgumentValues(CallSignature<ModifyStrArgs> call)
        {
            return ValidationScenario
                .For("STR", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.SourceString = s);
        }

        protected override string DoEvaluateResultType(CallSignature<ModifyStrArgs> call)
            => call.ValidatedArgs.SourceString.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ModifyStrArgs> call)
        {
            var str = call.ValidatedArgs.SourceString;

            if (str.IsNull)
            {
                call.Context.RedundantCall("Source string is NULL", str.Source);
                return str.TypeHandler.StrValueFactory.MakeNullValue(str.TypeName, call.Context.NewSource);
            }

            if (str.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Source string is empty", str.Source);
                return str;
            }

            if (!str.IsPreciseValue)
            {
                return str;
            }

            var modifiedString = ModifyString(str.Value);

            if (str.SourceKind != SqlValueSourceKind.Expression
            && str.EstimatedSize < MaxReadableLiteral
            && string.Equals(modifiedString, str.Value))
            {
                call.Context.RedundantCall($"'{str.Value}' {redundantModificationDescr}", str.Source);
            }

            return str.ChangeTo(modifiedString, call.Context.NewSource);
        }

        protected abstract string ModifyString(string originalValue);

        public class ModifyStrArgs
        {
            public SqlStrTypeValue SourceString { get; set; }
        }
    }
}
