using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IConditionalFlowHandler
    {
        bool DetectPredicatesLimitingVarValues(BooleanExpression predicate);

        bool DetectEqualityLimitingVarValues(ScalarExpression sourceValue, ScalarExpression limitDefinition);

        void ResetValueEstimatesAfterConditionalBlock(TSqlFragment block);

        void RevertValueEstimatesToBeforeBlock(TSqlFragment block);
    }
}
