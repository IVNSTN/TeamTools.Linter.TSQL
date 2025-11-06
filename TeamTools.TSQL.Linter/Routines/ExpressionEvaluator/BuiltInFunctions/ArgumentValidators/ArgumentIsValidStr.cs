using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValidStr
    {
        public static bool Validate(
            ArgumentValidation argData,
            SqlValue argValue,
            Action<SqlStrTypeValue> success)
        {
            var strValue = argData.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(argValue);

            if (strValue is null)
            {
                argData.Context.InvalidArgument(argData.ArgumentName, "Conversion to VARCHAR failed");
                return false;
            }

            success?.Invoke(strValue);

            return true;
        }
    }
}
