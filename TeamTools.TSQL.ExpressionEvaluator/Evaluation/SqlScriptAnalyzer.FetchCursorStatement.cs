using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// FETCH INTO var are assignments.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<FetchCursorStatement> evalFetchCursor;

        public override void Visit(FetchCursorStatement node)
        {
            if (node.IntoVariables.Count == 0)
            {
                return;
            }

            walkThrough.Run(node, evalFetchCursor ?? (evalFetchCursor = new Action<FetchCursorStatement>(EvalFetchCursor)));
        }

        private void EvalFetchCursor(FetchCursorStatement node)
        {
            MultiAssignDetector.Monitor(
                violations,
                callback => AnalyzeFetch(node.IntoVariables, callback));
        }

        private void AnalyzeFetch(IList<VariableReference> fetchedVars, Action<string, TSqlFragment> varAssignmentCallback)
        {
            int n = fetchedVars.Count;
            for (int i = 0; i < n; i++)
            {
                var fetchVar = fetchedVars[i];
                string varName = fetchVar.Name;

                // resetting value evaluations because we have no idea
                // what was fetched from cursor
                varRegistry.RegisterEvaluatedValue(varName, fetchVar.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, fetchVar));

                varAssignmentCallback(varName, fetchVar);
            }
        }
    }
}
