using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public sealed class MockEvaluator : IExpressionEvaluator
    {
        private readonly SqlValue magicValue;

        public MockEvaluator(SqlValue magicValue)
        {
            this.magicValue = magicValue;
        }

        public SqlValue EvaluateExpression(ScalarExpression node) => magicValue;

        public SqlValue EvaluateVariableModification(string varName, ScalarExpression expr, AssignmentKind operKind) => magicValue;
    }
}
