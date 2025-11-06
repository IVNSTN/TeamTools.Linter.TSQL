using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public class ArgumentIsDatePart
    {
        public static bool Validate(
           ArgumentValidation arg,
           Action<DatePartArgument> success)
        {
            if (arg?.Arg is null || !(arg.Arg is DatePartArgument datePartArg))
            {
                arg.Context.InvalidArgument(arg.ArgumentName);
                return false;
            }

            if (datePartArg.DatePartValue == DatePartEnum.Unknown)
            {
                if (!DatePartConverter.SupportedDateParts.Contains(datePartArg.DatePartName))
                {
                    arg.Context.InvalidArgument(arg.ArgumentName);
                }

                return false;
            }

            success(datePartArg);

            return true;
        }
    }
}
