using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class AllArgsAreValues
    {
        public static bool Validate(
            IList<SqlFunctionArgument> input,
            EvaluationContext context,
            Action<SqlValue> success)
        {
            bool allValid = true;

            for (int i = 0; i < input.Count; i++)
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
