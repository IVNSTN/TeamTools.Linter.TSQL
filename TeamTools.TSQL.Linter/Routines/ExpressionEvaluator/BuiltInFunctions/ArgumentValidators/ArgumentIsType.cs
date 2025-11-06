using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public class ArgumentIsType
    {
        public static bool Validate(
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
