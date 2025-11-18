using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class AllArgsAreValues
    {
        public static bool Validate(
            IList<SqlFunctionArgument> input,
            EvaluationContext context,
            Action<SqlValue> success)
        {
            bool allValid = true;

            for (int i = 0, n = input.Count; i < n; i++)
            {
                allValid = ArgumentIsValue.Validate(
                        new ArgumentValidation(i.ToString(), input[i], context),
                        v => success?.Invoke(v))
                    && allValid;
            }

            return allValid;
        }
    }
}
