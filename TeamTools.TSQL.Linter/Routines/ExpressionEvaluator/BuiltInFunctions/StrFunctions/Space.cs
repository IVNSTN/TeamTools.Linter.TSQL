using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // TODO : looks like subclass of Replicate
    public class Space : SqlGenericFunctionHandler<Space.GenerateSpacesArgs>
    {
        private static readonly int RequiredArgumentCount = 1;
        private static readonly int MaxLengthToGenerate = 200;
        private static readonly string FuncName = "SPACE";
        private static readonly string ResultTypeName = "dbo.VARCHAR";
        private static readonly SqlIntValueRange ValidCount = new SqlIntValueRange(0, int.MaxValue);

        public Space() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<GenerateSpacesArgs> call)
        {
            return ValidationScenario
                .For("SPACE_NUMBER", call.RawArgs[0], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .And<SqlIntTypeValue>((arg, val, success) => ArgumentIsValidInt.Validate(arg, val, success))
                .Then(i => call.ValidatedArgs.NumberOfSpaces = i);
        }

        protected override string DoEvaluateResultType(CallSignature<GenerateSpacesArgs> call) => ResultTypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<GenerateSpacesArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            if (call.ValidatedArgs.NumberOfSpaces.IsNull)
            {
                call.Context.RedundantCall("COUNT is NULL", call.ValidatedArgs.NumberOfSpaces.Source);
                return value.TypeHandler.StrValueFactory.MakeNullValue(call.ResultType, call.ValidatedArgs.NumberOfSpaces.Source);
            }

            if (call.ValidatedArgs.NumberOfSpaces.EstimatedSize.High == 0)
            {
                call.Context.RedundantCall("COUNT is zero", call.ValidatedArgs.NumberOfSpaces.Source);
                return value.TypeHandler.StrValueFactory.MakePreciseValue(call.ResultType, "", call.ValidatedArgs.NumberOfSpaces.Source);
            }

            if (call.ValidatedArgs.NumberOfSpaces.EstimatedSize.Low > ValidCount.High
            || call.ValidatedArgs.NumberOfSpaces.EstimatedSize.High < ValidCount.Low)
            {
                call.Context.InvalidArgument("COUNT", "Value is out of range");
                return value.TypeHandler.StrValueFactory.MakeNullValue(call.ResultType, call.ValidatedArgs.NumberOfSpaces.Source);
            }

            int estimatedLength = call.ValidatedArgs.NumberOfSpaces.EstimatedSize.High;

            if (call.ValidatedArgs.NumberOfSpaces.IsPreciseValue
            && estimatedLength <= MaxLengthToGenerate)
            {
                return value.ChangeTo(new string(' ', call.ValidatedArgs.NumberOfSpaces.Value), value.Source);
            }

            return value.ChangeTo(call.ValidatedArgs.NumberOfSpaces.EstimatedSize.High, value.Source);
        }

        public class GenerateSpacesArgs
        {
            public SqlIntTypeValue NumberOfSpaces { get; set; }
        }
    }
}
