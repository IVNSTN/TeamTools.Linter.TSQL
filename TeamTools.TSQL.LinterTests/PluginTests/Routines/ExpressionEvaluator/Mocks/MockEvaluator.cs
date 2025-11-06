using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

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
