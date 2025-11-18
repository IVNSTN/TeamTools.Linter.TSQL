using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    // TODO : support culture, format for CONVERT and so on - the third arg
    public abstract class ExplicitConvertionFunction : SqlGenericFunctionHandler<ExplicitConvertionFunction.ConvertArgs>
    {
        protected ExplicitConvertionFunction(string funcName, int requiredArgs = 2) : base(funcName, requiredArgs)
        {
        }

        private static string MsgSourceIsAlreadyOfThatType => Strings.ViolationDetails_RedundantTypeConversionViolation_ValueIsAlreadyOfThisType;

        public override bool ValidateArgumentValues(CallSignature<ConvertArgs> call)
        {
            // even if source value is unpredictable we can assume
            // that result will anyways be of given type definition
            // because this is an explicit conversion
            ValidationScenario
                .For("SRC", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(v => call.ValidatedArgs.SrcValue = v);

            return ValidationScenario
                .For("TYPE", call.RawArgs[1], call.Context)
                .When(ArgumentIsType.Validate)
                .Then(t => call.ValidatedArgs.TargetType = t);
        }

        protected override string DoEvaluateResultType(CallSignature<ConvertArgs> call) => call.ValidatedArgs.TargetType.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ConvertArgs> call)
        {
            if (call.ValidatedArgs.SrcValue is null)
            {
                return call.ValidatedArgs.TargetType.MakeUnknownValue();
            }

            if (call.ValidatedArgs.TargetType is SqlStrTypeReference str && str.IsUnicode
            && call.ValidatedArgs.SrcValue is SqlIntTypeValue intValue)
            {
                // TODO : If this conversion result is used in concatenation with other NVARCHAR strings
                // then no violation should be issued.
                string msg;

                if (call.ValidatedArgs.SrcValue.Source is SqlValueSourceVariable v)
                {
                    msg = string.Format(Strings.ViolationDetails_NumbersHaveNoUnicode_VarIsNumber, FunctionName, v.VarName);
                }
                else if (call.ValidatedArgs.SrcValue.SourceKind == SqlValueSourceKind.Literal && call.ValidatedArgs.SrcValue.IsPreciseValue)
                {
                    msg = string.Format(Strings.ViolationDetails_NumbersHaveNoUnicode_LiteralValueIsNumber, FunctionName, intValue.Value.ToString());
                }
                else
                {
                    msg = string.Format(Strings.ViolationDetails_NumbersHaveNoUnicode_ExpressionIsNumber, FunctionName);
                }

                call.Context.Violations.RegisterViolation(new NumbersHaveNoUnicodeViolation(msg, call.Context.NewSource));
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

            var violationMessage = string.Format(MsgSourceIsAlreadyOfThatType, srcValue.Source.ToString(), srcValue.TypeReference.ToString());

            context.Violations.RegisterViolation(new RedundantTypeConversionViolation(violationMessage, context.NewSource));
        }

        public class ConvertArgs
        {
            public SqlTypeReference TargetType { get; set; }

            public SqlValue SrcValue { get; set; }
        }
    }
}
