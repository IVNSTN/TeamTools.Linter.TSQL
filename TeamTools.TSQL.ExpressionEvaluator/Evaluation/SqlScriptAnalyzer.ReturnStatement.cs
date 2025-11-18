using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// RETURN [value] handler.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<ReturnStatement> returnEval;

        public override void Visit(ReturnStatement node)
        {
            walkThrough.Run(node, returnEval ?? (returnEval = new Action<ReturnStatement>(ReturnEval)));
        }

        private void ReturnEval(ReturnStatement node)
        {
            if (node.Expression is null)
            {
                return;
            }

            var ev = Evaluator.EvaluateExpression(node.Expression);
            if (ev is null)
            {
                return;
            }

            ev.Source = new SqlValueSource(ev.SourceKind, node.Expression);

            const string returnVarPseudoName = "RETURN_VALUE";
            VarRegistry.RegisterEvaluatedValue(returnVarPseudoName, node.LastTokenIndex, ev);
        }
    }
}
