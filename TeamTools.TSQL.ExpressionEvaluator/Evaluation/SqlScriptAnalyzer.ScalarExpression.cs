using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Validating all scalar expressions even if they are not involved in
    /// variable assignments. This includes built-in function calls.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<ScalarExpression> scalarEval;

        public override void Visit(ScalarExpression node)
        {
            walkThrough.Run(node, scalarEval ?? (scalarEval = new Action<ScalarExpression>(nd => _ = evaluator.EvaluateExpression(nd))));
        }
    }
}
