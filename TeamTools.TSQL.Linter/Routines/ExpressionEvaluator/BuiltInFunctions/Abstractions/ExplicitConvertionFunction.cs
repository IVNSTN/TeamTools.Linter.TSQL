using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    // TODO : support culture, format for CONVERT and so on - the third arg
    public abstract class ExplicitConvertionFunction : SqlGenericFunctionHandler<ExplicitConvertionFunction.ConvertArgs>
    {
        public ExplicitConvertionFunction(string funcName, int requiredArgs = 2) : base(funcName, requiredArgs)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ConvertArgs> call)
        {
            // even if source value is unpredictable we can assume
            // that result will anyways be of given type definition
            // because this is an explicit conversion
            ValidationScenario
                .For("SRC", call.RawArgs[0], call.Context)
                .When<SqlValue>(ArgumentIsValue.Validate)
                .Then(v => call.ValidatedArgs.SrcValue = v);

            return ValidationScenario
                .For("TYPE", call.RawArgs[1], call.Context)
                .When<SqlTypeReference>(ArgumentIsType.Validate)
                .Then(t => call.ValidatedArgs.TargetType = t);
        }

        protected override string DoEvaluateResultType(CallSignature<ConvertArgs> call) => call.ValidatedArgs.TargetType.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ConvertArgs> call)
        {
            if (call.ValidatedArgs.SrcValue is null)
            {
                return call.ValidatedArgs.TargetType.MakeUnknownValue();
            }

            if (call.ValidatedArgs.SrcValue.IsNull)
            {
                return call.ValidatedArgs.TargetType.MakeNullValue();
            }

            return call.Context.Converter.ExplicitlyConvertTo(
                call.ValidatedArgs.TargetType,
                call.ValidatedArgs.SrcValue,
                alreadyOfTypeName => RegisterRedundantConversionViolation(call.ValidatedArgs.SrcValue, call.Context));
        }

        private static void RegisterRedundantConversionViolation(SqlValue srcValue, EvaluationContext context)
        {
            if (srcValue.SourceKind == SqlValueSourceKind.Literal)
            {
                // Explicit type definition for hardcoded literal is fine
                // and is a preferred style for selected column definitions
                return;
            }

            var violationMessage = $"{srcValue.Source} is already of type {srcValue.TypeReference}";

            context.Violations.RegisterViolation(new RedundantTypeConversionViolation(violationMessage, context.NewSource));
        }

        public class ConvertArgs
        {
            public SqlTypeReference TargetType { get; set; }

            public SqlValue SrcValue { get; set; }
        }
    }
}
