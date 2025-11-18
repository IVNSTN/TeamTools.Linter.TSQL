using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    internal static class ConcatenationOutputTypeEvaluator
    {
        public static string DoEvaluateResultType<T>(CallSignature<T> call)
        where T : ConcatenationArgs, new()
        {
            if (ContainsUnicodeArg(call.ValidatedArgs.Values))
            {
                return TSqlDomainAttributes.Types.NVarchar;
            }
            else
            {
                return TSqlDomainAttributes.Types.Varchar;
            }
        }

        private static bool ContainsUnicodeArg(IList<SqlValue> args)
        {
            for (int i = args.Count - 1; i >= 0; i--)
            {
                if (args[i].TypeReference is SqlStrTypeReference str
                && str.IsUnicode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
