using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    public class Round : RoundingFunction<Round.RoundArgs>
    {
        private const int MaxRoundingDigit = 28;

        private static readonly string FuncName = "ROUND";
        private static readonly int MinArgumentCount = 2;
        private static readonly int MaxArgumentCount = 3;

        public Round() : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<RoundArgs> call)
        {
            return base.ValidateArgumentValues(call)
            && ValidationScenario
                .For("ROUND_LENGTH", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidInt.Validate)
                .Then(n => call.ValidatedArgs.Length = n);
        }

        protected override sealed SqlValue DoEvaluateResultValue(CallSignature<RoundArgs> call)
        {
            // For positive length the ancestor scenario is completely fine
            if (call.ValidatedArgs.Length.IsPreciseValue && !call.ValidatedArgs.Length.IsNull
            && call.ValidatedArgs.Length.Value >= 0)
            {
                return base.DoEvaluateResultValue(call);
            }

            var value = call.Context.Converter.ImplicitlyConvert<SqlDecimalTypeValue>(call
                .ResultTypeHandler
                .MakeSqlDataTypeReference(call.ResultType)
                .MakeUnknownValue());

            // TODO : if Length has limited value range, e.g. 0-MaxInt then it is not negative for sure
            // more accurate approximation can be made
            if (value is null || !call.ValidatedArgs.SourceValue.IsPreciseValue || !call.ValidatedArgs.Length.IsPreciseValue)
            {
                return value;
            }

            if (call.ValidatedArgs.SourceValue.IsNull || call.ValidatedArgs.Length.IsNull)
            {
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node);
            }

            if (Math.Abs(call.ValidatedArgs.Length.Value) > MaxRoundingDigit)
            {
                // .net supports rounding for 0-28 digits only
                return value;
            }

            decimal srcValue;

            // we already checked that SourceValue it is precise
            if (call.ValidatedArgs.SourceValue is SqlDecimalTypeValue dec)
            {
                srcValue = dec.Value;
            }
            else if (call.ValidatedArgs.SourceValue is SqlIntTypeValue i)
            {
                srcValue = (decimal)i.Value;
            }
            else if (call.ValidatedArgs.SourceValue is SqlBigIntTypeValue b)
            {
                srcValue = (decimal)b.Value;
            }
            else
            {
                // TODO : return precise value if possible or a limited value range
                return value;
            }

            return value.ChangeTo(ProduceRounding(srcValue, call.ValidatedArgs), call.Context.NewSource);
        }

        protected override decimal ProduceRounding(decimal value, RoundArgs arguments)
        {
            if (arguments.Length.Value >= 0)
            {
                return Math.Round(value, arguments.Length.Value);
            }

            var pow = (decimal)Math.Pow(10, -arguments.Length.Value);

            return Math.Round(value / pow, 0) * pow;
        }

        [ExcludeFromCodeCoverage]
        public sealed class RoundArgs : RoundingFunctionArgs
        {
            public SqlIntTypeValue Length { get; set; }
        }
    }
}
