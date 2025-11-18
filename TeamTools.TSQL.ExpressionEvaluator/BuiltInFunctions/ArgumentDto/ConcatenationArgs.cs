using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    public class ConcatenationArgs
    {
        public ConcatenationArgs()
        { }

        public List<SqlValue> Values { get; } = new List<SqlValue>();
    }
}
