using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
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
