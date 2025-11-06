using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ArgumentIsValidInt
    {
        public static bool Validate(
            ArgumentValidation argData,
            SqlValue argValue,
            Action<SqlIntTypeValue> success,
            SqlIntValueRange validRange = null)
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
