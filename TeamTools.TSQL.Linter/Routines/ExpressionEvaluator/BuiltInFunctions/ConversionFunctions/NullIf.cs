using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class NullIf : SqlGenericFunctionHandler<NullIf.NullIfArgs>
    {
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string FuncName = "NULLIF";
        private static readonly int MaxReadableLength = 32;

        public NullIf() : base(FuncName, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<NullIfArgs> call)
        {
            return ValidationScenario
                    .For("SRC", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.FirstArg = v)
                && ValidationScenario
                    .For("NULL_VALUE", call.RawArgs[1], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.SecondArg = v);
        }

        // NULLIF result type always equals first argument type
        protected override string DoEvaluateResultType(CallSignature<NullIfArgs> call)
            => call.ValidatedArgs.FirstArg.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<NullIfArgs> call)
        {
            var value = base.DoEvaluateResultValue(call);

            if (value is null)
            {
                return default;
            }

            if (call.ValidatedArgs.FirstArg.IsNull || call.ValidatedArgs.SecondArg.IsNull)
            {
                call.Context.RedundantCall("First and second args are not supposed to be NULL");
                return call.ValidatedArgs.FirstArg;
            }

            if (call.ValidatedArgs.FirstArg.Source is SqlValueSourceVariable firstVar
            && call.ValidatedArgs.SecondArg.Source is SqlValueSourceVariable secondVar
            && string.Equals(firstVar.VarName, secondVar.VarName, StringComparison.OrdinalIgnoreCase))
            {
                call.Context.RedundantCall("Same var on both sides");
                return value.TypeReference.MakeNullValue();
            }

            var second = call.Context.Converter.ImplicitlyConvertTo(call.ResultType, call.ValidatedArgs.SecondArg);

            if (second is null)
            {
                call.Context.InvalidArgument($"Second argument '{call.ValidatedArgs.SecondArg.TypeName}' cannot be converted to '{call.ResultType}'");
                return default;
            }

            // TODO : and where is SqlValue.Compare / SqlValue.Equals method?
            // FIXME : get rid of hardcoded type casting
            if (call.ValidatedArgs.FirstArg is SqlIntTypeValue firstInt
            && second is SqlIntTypeValue secondInt)
            {
                if (firstInt.IsPreciseValue && secondInt.IsPreciseValue)
                {
                    if (firstInt.Value == secondInt.Value)
                    {
                        call.Context.RedundantCall($"Values are equal '{firstInt.Value}' for sure, result is always NULL");
                        return value.TypeReference.MakeNullValue();
                    }
                    else
                    {
                        call.Context.RedundantCall("Values are not equal for sure");
                        return call.ValidatedArgs.FirstArg;
                    }
                }
                else if (secondInt.IsPreciseValue
                && (secondInt.Value > firstInt.EstimatedSize.High
                || secondInt.Value < firstInt.EstimatedSize.Low))
                {
                    call.Context.RedundantCall($"First arg can never be equal to {secondInt.Value}");
                    return call.ValidatedArgs.FirstArg;
                }
                else if (firstInt.IsPreciseValue
                && (firstInt.Value > secondInt.EstimatedSize.High
                || firstInt.Value < secondInt.EstimatedSize.Low))
                {
                    call.Context.RedundantCall($"Second arg can never be equal to {firstInt.Value}");
                    return call.ValidatedArgs.FirstArg;
                }
                else if (firstInt.EstimatedSize.Low > secondInt.EstimatedSize.High
                || firstInt.EstimatedSize.High < secondInt.EstimatedSize.Low)
                {
                    call.Context.RedundantCall($"Second arg can never be equal");
                    return call.ValidatedArgs.FirstArg;
                }
            }
            else if (call.ValidatedArgs.FirstArg is SqlStrTypeValue firstStr
            && second is SqlStrTypeValue secondStr)
            {
                if (firstStr.IsPreciseValue && secondStr.IsPreciseValue)
                {
                    if (string.Equals(firstStr.Value, secondStr.Value))
                    {
                        string valueRepresentation;
                        if (firstStr.Value.Length > MaxReadableLength)
                        {
                            valueRepresentation = $"{firstStr.Value.Length}-long strings";
                        }
                        else
                        {
                            valueRepresentation = $"'{firstStr.Value}'";
                        }

                        call.Context.RedundantCall($"Values are equal {valueRepresentation} for sure, result is always NULL");
                        return value.TypeReference.MakeNullValue();
                    }
                    else
                    {
                        call.Context.RedundantCall("Values are not equal for sure");
                        return call.ValidatedArgs.FirstArg;
                    }
                }
                else if (secondStr.IsPreciseValue
                && secondStr.Value.Length > firstStr.EstimatedSize)
                {
                    call.Context.RedundantCall($"First arg cannot have {secondStr.Value.Length} chars");
                    return call.ValidatedArgs.FirstArg;
                }
                else if (firstStr.IsPreciseValue
                && firstStr.Value.Length > secondStr.EstimatedSize)
                {
                    call.Context.RedundantCall($"Second arg cannot have {firstStr.Value.Length} chars");
                    return call.ValidatedArgs.FirstArg;
                }
            }

            return value;
        }

        public class NullIfArgs
        {
            public SqlValue FirstArg { get; set; }

            public SqlValue SecondArg { get; set; }
        }
    }
}
