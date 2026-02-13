using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    public class Ceiling : RoundingFunction<Ceiling.CeilingArgs>
    {
        private static readonly string FuncName = "CEILING";

        public Ceiling() : base(FuncName)
        { }

        protected override decimal ProduceRounding(decimal value, CeilingArgs arguments) => Math.Ceiling(value);

        [ExcludeFromCodeCoverage]
        public sealed class CeilingArgs : RoundingFunctionArgs
        { }
    }
}
