using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IExpressionEvaluator
    {
        SqlValue EvaluateExpression(ScalarExpression node);

        [Obsolete("This crutch is used at single place")]
        SqlValue EvaluateVariableModification(string varName, ScalarExpression expr, AssignmentKind operKind);
    }
}
