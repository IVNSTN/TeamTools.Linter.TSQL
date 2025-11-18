using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateBuiltInExpressionExtensions
    {
        public static SqlValue EvaluateBuiltInExpression(this SqlExpressionEvaluator eval, PrimaryExpression expr)
        {
            if (expr is LeftFunctionCall lf)
            {
                return eval.EvaluateFunctionResult(
                    "LEFT",
                    eval.ToArgs(lf.Parameters),
                    expr);
            }

            if (expr is RightFunctionCall rf)
            {
                return eval.EvaluateFunctionResult(
                    "RIGHT",
                    eval.ToArgs(rf.Parameters),
                    expr);
            }

            if (expr is CoalesceExpression coal)
            {
                return eval.EvaluateFunctionResult(
                    "COALESCE",
                    eval.ToArgs(coal.Expressions),
                    expr);
            }

            if (expr is CastCall cast)
            {
                return eval.EvaluateFunctionResult(
                    "CAST",
                    eval.ToArgs(eval.ToArg(cast.Parameter), eval.ToArg(cast.DataType)),
                    expr);
            }

            if (expr is ConvertCall cnv)
            {
                return eval.EvaluateFunctionResult(
                    "CONVERT",
                    eval.ToArgs(eval.ToArg(cnv.Parameter), eval.ToArg(cnv.DataType)),
                    expr);
            }

            if (expr is ParseCall prs)
            {
                return eval.EvaluateFunctionResult(
                    "PARSE",
                    eval.ToArgs(eval.ToArg(prs.StringValue), eval.ToArg(prs.DataType)),
                    expr);
            }

            if (expr is TryCastCall tcast)
            {
                return eval.EvaluateFunctionResult(
                    "TRY_CAST",
                    eval.ToArgs(eval.ToArg(tcast.Parameter), eval.ToArg(tcast.DataType)),
                    expr);
            }

            if (expr is TryConvertCall tcnv)
            {
                return eval.EvaluateFunctionResult(
                    "TRY_CONVERT",
                    eval.ToArgs(eval.ToArg(tcnv.Parameter), eval.ToArg(tcnv.DataType)),
                    expr);
            }

            if (expr is TryParseCall tprs)
            {
                return eval.EvaluateFunctionResult(
                    "TRY_PARSE",
                    eval.ToArgs(eval.ToArg(tprs.StringValue), eval.ToArg(tprs.DataType)),
                    expr);
            }

            if (expr is IIfCall iif)
            {
                return eval.EvaluateFunctionResult(
                    "IIF",
                    eval.ToArgs(eval.ToArg(iif.Predicate), eval.ToArg(iif.ThenExpression), eval.ToArg(iif.ElseExpression)),
                    expr);
            }

            if (expr is NullIfExpression nlif)
            {
                return eval.EvaluateFunctionResult(
                    "NULLIF",
                    eval.ToArgs(eval.ToArg(nlif.FirstExpression), eval.ToArg(nlif.SecondExpression)),
                    expr);
            }

            return default;
        }

        public static List<SqlFunctionArgument> ToArgs(this SqlExpressionEvaluator eval, IList<ScalarExpression> srcParams)
        {
            int n = srcParams?.Count ?? 0;
            var output = new List<SqlFunctionArgument>(n);

            for (int i = 0; i < n; i++)
            {
                output.Add(eval.ToArg(srcParams[i]));
            }

            return output;
        }

        public static List<SqlFunctionArgument> ToArgs(this SqlExpressionEvaluator eval, params SqlFunctionArgument[] srcParams) => srcParams.ToList();

        public static SqlFunctionArgument ToArg(this SqlExpressionEvaluator eval, ScalarExpression srcParam)
        {
            if (srcParam is null)
            {
                return default;
            }

            if (srcParam is ColumnReferenceExpression colRef)
            {
                return eval.ToArg(colRef);
            }

            return new ValueArgument(eval.EvaluateExpression(srcParam));
        }

        public static SqlFunctionArgument ToArg(this SqlExpressionEvaluator eval, BooleanExpression srcPredicate)
        {
            if (srcPredicate is null)
            {
                return default;
            }

            // TODO : evaluate
            return new PredicateArgument(srcPredicate);
        }

        public static SqlFunctionArgument ToArg(this SqlExpressionEvaluator eval, DataTypeReference dataType)
        {
            if (dataType is null)
            {
                return default;
            }

            return new TypeArgument(eval.TypeResolver.ResolveType(dataType));
        }

        // Date part names are parsed as column references
        public static SqlFunctionArgument ToArg(this SqlExpressionEvaluator eval, ColumnReferenceExpression datePart)
        {
            if (datePart?.MultiPartIdentifier is null || datePart.MultiPartIdentifier.Identifiers.Count != 1)
            {
                return default;
            }

            string datePartNameCandidate = datePart.MultiPartIdentifier.Identifiers[0].Value;

            if (string.IsNullOrEmpty(datePartNameCandidate)
            || !DatePartConverter.SupportedDateParts.ContainsKey(datePartNameCandidate))
            {
                // looks like a real column reference
                return new ValueArgument(default);
            }

            return new DatePartArgument(datePartNameCandidate);
        }
    }
}
