using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValidStr
    {
        public static Func<
            ArgumentValidation,
            SqlValue,
            Action<SqlStrTypeValue>,
            bool> Validate
        { get; } = new Func<ArgumentValidation, SqlValue, Action<SqlStrTypeValue>, bool>(DoValidate);

        private static bool DoValidate(
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
