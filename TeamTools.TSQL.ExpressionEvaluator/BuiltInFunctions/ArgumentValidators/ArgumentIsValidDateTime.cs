using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValidDateTime
    {
        public static Func<
            ArgumentValidation,
            SqlValue,
            Action<SqlDateTimeValue>,
            bool> Validate
        { get; } = new Func<ArgumentValidation, SqlValue, Action<SqlDateTimeValue>, bool>(DoValidate);

        private static bool DoValidate(
            ArgumentValidation argData,
            SqlValue argValue,
            Action<SqlDateTimeValue> success)
        {
            var datetimeValue = argData.Context.Converter.ImplicitlyConvert<SqlDateTimeValue>(argValue);

            if (datetimeValue is null)
            {
                argData.Context.InvalidArgument(argData.ArgumentName, "Conversion to DATETIME failed");
                return false;
            }

            success?.Invoke(datetimeValue);

            return true;
        }
    }
}
