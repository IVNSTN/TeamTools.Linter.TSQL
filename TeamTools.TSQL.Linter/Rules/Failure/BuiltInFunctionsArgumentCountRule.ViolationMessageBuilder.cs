using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Violation message builder.
    /// </summary>
    internal partial class BuiltInFunctionsArgumentCountRule
    {
        [ExcludeFromCodeCoverage]
        private string BuildViolationDetails(string functionName, int providedParamCount)
        {
            if (builtInFnArgCount[functionName].ParamCount >= 0)
            {
                return string.Format(
                    "{0} expectects {1} params but {2} passed",
                    functionName,
                    builtInFnArgCount[functionName].ParamCount.ToString(),
                    providedParamCount);
            }

            if (builtInFnArgCount[functionName].ParamCountMax >= 0)
            {
                if (builtInFnArgCount[functionName].ParamCountMin >= 0)
                {
                    return string.Format(
                        "{0} expects {1}-{2} params but {3} passed",
                        functionName,
                        builtInFnArgCount[functionName].ParamCountMin,
                        builtInFnArgCount[functionName].ParamCountMax,
                        providedParamCount);
                }

                return string.Format(
                    "{0} expects <={1} params but {2} passed",
                    functionName,
                    builtInFnArgCount[functionName].ParamCountMax,
                    providedParamCount);
            }

            if (builtInFnArgCount[functionName].ParamCountMin >= 0)
            {
                return string.Format(
                    "{0} expects >={1} params but {2} passed",
                    functionName,
                    builtInFnArgCount[functionName].ParamCountMin,
                    providedParamCount);
            }

            return default;
        }
    }
}
