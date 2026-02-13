using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class RoundingFunction<TArgs> : SqlGenericFunctionHandler<TArgs>
    where TArgs : RoundingFunction<TArgs>.RoundingFunctionArgs, new()
    {
        private static readonly int DefaultRequiredArgumentCount = 1;
        private static readonly string FallbackResultType = TSqlDomainAttributes.Types.Int;

        protected RoundingFunction(string funcName, int minArgs, int maxArgs) : base(funcName, minArgs, maxArgs)
        {
        }

        protected RoundingFunction(string funcName) : base(funcName, DefaultRequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<TArgs> call)
        {
            return ValidationScenario
                .For("SOURCE_VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(n => call.ValidatedArgs.SourceValue = n);
        }

        protected override string DoEvaluateResultType(CallSignature<TArgs> call)
        {
            if (call.ValidatedArgs.SourceValue is SqlIntTypeValue i)
            {
                // For int types output type is always INT
                // TODO : except for BIT - it is FLOAT
                // docs: https://learn.microsoft.com/en-us/sql/t-sql/functions/ceiling-transact-sql?view=sql-server-ver17#return-types
                return i.TypeHandler.IntValueFactory.FallbackTypeName;
            }

            if (call.ValidatedArgs.SourceValue is SqlBigIntTypeValue
            || call.ValidatedArgs.SourceValue is SqlDecimalTypeValue)
            {
                return call.ValidatedArgs.SourceValue.TypeName;
            }

            return FallbackResultType;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<TArgs> call)
        {
            // TODO : could be int or bigint
            var value = call.Context.Converter.ImplicitlyConvert<SqlDecimalTypeValue>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return value;
            }

            if (call.ValidatedArgs.SourceValue.IsNull)
            {
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node);
            }

            if (call.ValidatedArgs.SourceValue is SqlIntTypeValue i)
            {
                call.Context.RedundantCall("Source is a number without fractions");

                // the result is the same as source
                return value.ChangeTo((decimal)i.Value, call.Context.NewSource);
            }

            if (call.ValidatedArgs.SourceValue is SqlBigIntTypeValue bi)
            {
                call.Context.RedundantCall("Source is a number without fractions");

                // the result is the same as source
                return value.ChangeTo((decimal)bi.Value, call.Context.NewSource);
            }

            if (call.ValidatedArgs.SourceValue is SqlDecimalTypeValue decSrc)
            {
                if (decSrc.EstimatedSize.Scale == 0)
                {
                    call.Context.RedundantCall("Source is a number without fractions");
                }

                if (decSrc.IsPreciseValue)
                {
                    return value.ChangeTo(ProduceRounding(decSrc.Value, call.ValidatedArgs), call.Context.NewSource);
                }
            }

            // TODO : return precise value if possible or a limited value range
            return value;
        }

        protected abstract decimal ProduceRounding(decimal value, TArgs arguments);

        public class RoundingFunctionArgs
        {
            public SqlValue SourceValue { get; set; }
        }
    }
}
