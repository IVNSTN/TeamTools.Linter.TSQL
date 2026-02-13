using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions
{
    public class Floor : RoundingFunction<Floor.FloorArgs>
    {
        private static readonly string FuncName = "FLOOR";

        public Floor() : base(FuncName)
        { }

        protected override decimal ProduceRounding(decimal value, FloorArgs arguments) => Math.Floor(value);

        [ExcludeFromCodeCoverage]
        public sealed class FloorArgs : RoundingFunctionArgs
        { }
    }
}
