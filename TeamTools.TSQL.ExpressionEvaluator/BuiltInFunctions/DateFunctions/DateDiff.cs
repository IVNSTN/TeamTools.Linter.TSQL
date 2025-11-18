using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DateDiff : SqlFunctionHandler
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly string FuncName = "DATEDIFF";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;

        public DateDiff() : base(FuncName, RequiredArgumentCount)
        { }

        public override SqlValue Evaluate(List<SqlFunctionArgument> args, EvaluationContext context)
        {
            return context.TypeResolver.ResolveType(OutputType).MakeUnknownValue();
        }
    }
}
