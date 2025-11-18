using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValue
    {
        public static Func<ArgumentValidation, Action<SqlValue>, bool> Validate { get; } = new Func<ArgumentValidation, Action<SqlValue>, bool>(DoValidate);

        private static bool DoValidate(
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
