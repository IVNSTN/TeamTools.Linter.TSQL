using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Coalesce : CaseBasedResultFunctionHandler
    {
        private static readonly int MinArgumentCount = 2;
        private static readonly string FuncName = "COALESCE";

        public Coalesce() : base(FuncName, MinArgumentCount, int.MaxValue)
        {
        }

        protected override SqlValue EvaluateValuesToSpecificResult(IList<SqlValue> values, EvaluationContext context)
        {
            bool notNullFound = false;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].IsNull)
                {
                    context.RedundantCall($"arg {i} is always NULL", values[i].Source);
                }
                else if (notNullFound)
                {
                    context.RedundantCall($"arg {i} is redundant. One of prior args is never NULL", values[i].Source);
                }
                else if (values[i].IsPreciseValue)
                {
                    notNullFound = true;
                    if (i == 0)
                    {
                        // first arg is never null => this is the result
                        return values[i];
                    }
                }
            }

            return default;
        }
    }
}
