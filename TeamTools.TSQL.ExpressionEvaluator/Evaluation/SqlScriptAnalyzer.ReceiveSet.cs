using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// RECEIVE @var = value assignments processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<ReceiveStatement> evalReceive;
        private Action<WaitForStatement> evalWaitFor;

        public override void Visit(ReceiveStatement node)
        {
            walkThrough.Run(node, evalReceive ?? (evalReceive = new Action<ReceiveStatement>(ReceiveEval)));
        }

        public override void Visit(WaitForStatement node)
        {
            // If WAITFOR has TIMEOUT then it somehow will be visited before the inner waitfor statement
            walkThrough.Run(node, evalWaitFor ?? (evalWaitFor = new Action<WaitForStatement>(WaitForEval)));
        }

        private static IEnumerable<SelectSetVariable> ExtractSetVariable(IList<SelectElement> sel)
        {
            int n = sel.Count;
            for (int i = 0; i < n; i++)
            {
                if (sel[i] is SelectSetVariable selVar && selVar.Variable != null)
                {
                    yield return selVar;
                }
            }
        }

        private void ReceiveEval(ReceiveStatement node)
        {
            var setVarElements = ExtractSetVariable(node.SelectElements)
                .ToList();

            if (setVarElements.Count == 0)
            {
                return;
            }

            MultiAssignDetector.Monitor(
                violations,
                callback =>
                {
                    AnalyzeQuerySpecification(setVarElements, callback);

                    // A queue could be empty
                    ResetAssignmentsAfterConditionalSelect(node);
                });
        }

        private void WaitForEval(WaitForStatement node)
        {
            node.Statement?.Accept(this);
        }
    }
}
