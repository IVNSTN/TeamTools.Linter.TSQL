using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IExpressionEvaluator
    {
        SqlValue EvaluateExpression(ScalarExpression node);

        [Obsolete("This crutch is used at single place")]
        SqlValue EvaluateVariableModification(string varName, ScalarExpression expr, AssignmentKind operKind);
    }
}
