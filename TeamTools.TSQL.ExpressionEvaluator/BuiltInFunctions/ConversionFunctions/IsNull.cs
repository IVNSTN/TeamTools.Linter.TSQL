using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class IsNull : SqlGenericFunctionHandler<IsNull.IsNullArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string FuncName = "ISNULL";

        public IsNull() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<IsNullArgs> call)
        {
            return ValidationScenario
                    .For("SRC", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.SrcValue = v)
                && ValidationScenario
                    .For("NULL_VALUE", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.NullValue = v);
        }

        // ISNULL result type always equals first argument specific type and size
        protected override string DoEvaluateResultType(CallSignature<IsNullArgs> call)
            => call.ValidatedArgs.SrcValue.TypeName;

        // TODO : extract to something like strategy?
        protected override SqlValue DoEvaluateResultValue(CallSignature<IsNullArgs> call)
        {
            if (call.ValidatedArgs.NullValue.IsNull)
            {
                call.Context.RedundantCall("value for null replacement is supposed to be not null");
                return call.ValidatedArgs.SrcValue;
            }

            SqlTypeReference resultType = call.ValidatedArgs.SrcValue.TypeReference;
            SqlValue resultReplacement;

            // Doing convertions before returning any result to catch all possible
            // type incompatibility violations
            if (call.ValidatedArgs.SrcValue.SourceKind == SqlValueSourceKind.Variable)
            {
                // result will be explicitly of variable type
                if (call.ValidatedArgs.SrcValue.Source != null
                && call.ValidatedArgs.SrcValue.Source is SqlValueSourceVariable srcVar)
                {
                    // TODO : else - invalid arg violation?
                    resultType = call.Context.Variables.GetVariableTypeReference(srcVar.VarName);
                }

                resultReplacement = call.Context.Converter.ImplicitlyConvertTo(resultType, call.ValidatedArgs.NullValue);
            }
            else if (call.ValidatedArgs.SrcValue.Source != null
            && call.ValidatedArgs.SrcValue.SourceKind == SqlValueSourceKind.Expression
            && !(call.ValidatedArgs.SrcValue is SqlStrTypeValue))
            {
                // FIXME : not applicable to string/binary types
                var convertionTargetType = call.Context.TypeResolver
                    .ResolveTypeHandler(resultType.TypeName)?
                    .MakeSqlDataTypeReference(resultType.TypeName);

                // result will be in expression system TYPE range,
                // not in first arg VALUE range
                resultReplacement = call.Context.Converter.ImplicitlyConvertTo(convertionTargetType, call.ValidatedArgs.NullValue);
            }
            else if (call.ValidatedArgs.SrcValue is SqlStrTypeValue
            && call.ValidatedArgs.SrcValue.SourceKind != SqlValueSourceKind.Literal)
            {
                // FIXME : Use source value type from function/variable definition
                // and their combination. Not the "type" of approximate/precise value.
                resultReplacement = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(call.ValidatedArgs.NullValue);
            }
            else
            {
                resultReplacement = call.Context.Converter.ImplicitlyConvertTo(resultType, call.ValidatedArgs.NullValue);
            }

            if (call.ValidatedArgs.SrcValue.IsPreciseValue && !call.ValidatedArgs.SrcValue.IsNull)
            {
                call.Context.RedundantCall("first arg is never NULL");
                return call.ValidatedArgs.SrcValue;
            }

            if (resultReplacement is null)
            {
                // could not convert second arg to first arg type
                return resultType.MakeUnknownValue();
            }

            if (call.ValidatedArgs.SrcValue.IsNull)
            {
                call.Context.RedundantCall("first arg is always NULL");

                return resultReplacement;
            }

            return resultType.MakeUnknownValue();
        }

        public class IsNullArgs
        {
            public SqlValue SrcValue { get; set; }

            public SqlValue NullValue { get; set; }
        }
    }
}
