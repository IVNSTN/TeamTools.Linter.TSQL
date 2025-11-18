using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValidInt
    {
        private static readonly SqlIntValueRange PositiveInt = new SqlIntValueRange(1, int.MaxValue);

        public static Func<
            ArgumentValidation,
            SqlValue,
            Action<SqlIntTypeValue>,
            bool> Validate
        { get; } = new Func<ArgumentValidation, SqlValue, Action<SqlIntTypeValue>, bool>(DoValidate);

        public static Func<
            ArgumentValidation,
            SqlValue,
            Action<SqlIntTypeValue>,
            bool> ValidatePositiveInt
        { get; } = new Func<ArgumentValidation, SqlValue, Action<SqlIntTypeValue>, bool>((a, b, c) => DoValidate(a, b, c, PositiveInt));

        public static Func<
            ArgumentValidation,
            SqlValue,
            Action<SqlIntTypeValue>,
            SqlIntValueRange,
            bool> ValidateWithinRange
        { get; } = new Func<ArgumentValidation, SqlValue, Action<SqlIntTypeValue>, SqlIntValueRange, bool>(DoValidate);

        private static bool DoValidate(
            ArgumentValidation argData,
            SqlValue argValue,
            Action<SqlIntTypeValue> success)
        {
            return DoValidate(argData, argValue, success, null);
        }

        private static bool DoValidate(
            ArgumentValidation argData,
            SqlValue argValue,
            Action<SqlIntTypeValue> success,
            SqlIntValueRange validRange)
        {
            var intValue = argData.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(argValue);

            if (intValue is null)
            {
                argData.Context.InvalidArgument(argData.ArgumentName, "Conversion to INT failed");
                return false;
            }

            if (validRange != null
            && !intValue.IsNull
            && (intValue.EstimatedSize.Low < validRange.Low || intValue.EstimatedSize.High > validRange.High))
            {
                // if the value is not precise and at least one range bound
                // fits valid range then still trying to evaluate approximate function result
                if (intValue.IsPreciseValue
                || intValue.EstimatedSize.High < validRange.Low || intValue.EstimatedSize.Low > validRange.High)
                {
                    argData.Context.ArgumentOutOfRange(argData.ArgumentName);
                    return false;
                }
            }

            success?.Invoke(intValue);

            return true;
        }
    }
}
