using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsType
    {
        public static Func<ArgumentValidation, Action<SqlTypeReference>, bool> Validate { get; } = new Func<ArgumentValidation, Action<SqlTypeReference>, bool>(DoValidate);

        private static bool DoValidate(
            ArgumentValidation argData,
            Action<SqlTypeReference> success)
        {
            if (argData?.Arg is null)
            {
                // could not parse or unsupported value type
                return false;
            }

            if (!(argData.Arg is TypeArgument typeArg))
            {
                argData.Context.InvalidArgument(argData.ArgumentName);
                return false;
            }

            if (typeArg.TypeRef is null)
            {
                // could not parse or unsupported value type
                return false;
            }

            success?.Invoke(typeArg.TypeRef);

            return true;
        }
    }
}
