using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Coalesce : CaseBasedResultFunctionHandler
    {
        private static readonly int MinArgumentCount = 2;
        private static readonly string FuncName = "COALESCE";

        public Coalesce() : base(FuncName, MinArgumentCount, int.MaxValue)
        {
        }

        protected override SqlValue EvaluateValuesToSpecificResult(List<SqlValue> values, EvaluationContext context)
        {
            bool notNullFound = false;

            for (int i = 0, n = values.Count; i < n; i++)
            {
                var v = values[i];
                if (v.IsNull)
                {
                    context.RedundantCall($"arg {i} is always NULL", v.Source);
                }
                else if (notNullFound)
                {
                    context.RedundantCall($"arg {i} is redundant. One of prior args is never NULL", v.Source);
                }
                else if (v.IsPreciseValue)
                {
                    notNullFound = true;
                    if (i == 0)
                    {
                        // first arg is never null => this is the result
                        return v;
                    }
                }
            }

            return default;
        }
    }
}
