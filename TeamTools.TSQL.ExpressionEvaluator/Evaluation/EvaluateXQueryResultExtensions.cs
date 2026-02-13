using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateXQueryResultExtensions
    {
        public static SqlValue EvaluateXQueryResult(
            this SqlExpressionEvaluator eval,
            string functionName,
            List<SqlFunctionArgument> args,
            TSqlFragment node)
        {
            return eval.EvaluateFunctionResult("XQuery." + functionName, args, node);
        }
    }
}
