using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsDatePart
    {
        public static Func<ArgumentValidation, Action<DatePartArgument>, bool> Validate { get; } = new Func<ArgumentValidation, Action<DatePartArgument>, bool>(DoValidate);

        private static bool DoValidate(
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
                if (!DatePartConverter.SupportedDateParts.ContainsKey(datePartArg.DatePartName))
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
