using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateFunctionResultExtensions
    {
        public static SqlValue EvaluateFunctionResult(
            this SqlExpressionEvaluator eval,
            string functionName,
            List<SqlFunctionArgument> args,
            TSqlFragment node)
        {
            var fn = eval.FunctionRegistry.GetFunction(functionName);

            if (fn is null)
            {
                return default;
            }

            var context = new EvaluationContext(
                eval,
                eval.Converter,
                eval.TypeResolver,
                eval.VariableEvaluator,
                eval.Violations,
                functionName,
                node);

            var ev = fn.Evaluate(args, context);

            if (ev is null)
            {
                return default;
            }

            var sourceKind = SqlValueSourceKind.Expression;
            // TODO : Too much magic. And not sure if this is the right place.
            if (ev.IsPreciseValue && ev.SourceKind == SqlValueSourceKind.Literal
            && !args.Any(a => a is ValueArgument v && v.Value?.SourceKind != SqlValueSourceKind.Literal))
            {
                sourceKind = SqlValueSourceKind.Literal;
            }

            ev.Source = new SqlValueSource(sourceKind, context.Node);

            return ev;
        }
    }
}
