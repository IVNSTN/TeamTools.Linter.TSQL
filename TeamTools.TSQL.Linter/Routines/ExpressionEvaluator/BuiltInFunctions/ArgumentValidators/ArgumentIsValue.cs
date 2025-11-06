using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValue
    {
        public static bool Validate(
            ArgumentValidation argData,
            Action<SqlValue> success)
        {
            if (argData?.Arg is null)
            {
                // could not parse or unsupported value type
                return false;
            }

            if (!(argData.Arg is ValueArgument argValue))
            {
                // TODO : or column reference
                if (!(argData.Arg is DatePartArgument))
                {
                    argData.Context.InvalidArgument(argData.ArgumentName);
                }

                return false;
            }

            if (argValue.Value is null)
            {
                // could not parse or unsupported value type
                return false;
            }

            success?.Invoke(argValue.Value);

            return true;
        }
    }
}
