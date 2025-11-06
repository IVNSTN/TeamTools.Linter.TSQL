using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateFunctionResultExtensions
    {
        public static SqlValue EvaluateFunctionResult(
            this SqlExpressionEvaluator eval,
            string functionName,
            IList<SqlFunctionArgument> args,
            TSqlFragment node)
        {
            if (!eval.FunctionRegistry.IsFunctionRegistered(functionName))
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

            var ev = eval.FunctionRegistry.GetFunction(functionName).Evaluate(args, context);
            if (ev != null)
            {
                var sourceKind = SqlValueSourceKind.Expression;
                // TODO : Too much magic. And not sure if this is the right place.
                if (ev.IsPreciseValue && ev.SourceKind == SqlValueSourceKind.Literal
                && !args.Any(a => a is ValueArgument v && v.Value?.SourceKind != SqlValueSourceKind.Literal))
                {
                    sourceKind = SqlValueSourceKind.Literal;
                }

                ev.Source = new SqlValueSource(sourceKind, context.Node);
            }

            return ev;
        }
    }
}
