using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Core
{
    [ExcludeFromCodeCoverage]
    public class EvaluatedValueAtPos
    {
        public EvaluatedValueAtPos(int startingFromTokenIndex, SqlValue evaluatedValue)
        {
            StartingFromTokenIndex = startingFromTokenIndex;
            EvaluatedValue = evaluatedValue;
        }

        public int StartingFromTokenIndex { get; }

        public SqlValue EvaluatedValue { get; }
    }
}
