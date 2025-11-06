using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// FETCH INTO var are assignments.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(FetchCursorStatement node)
        {
            if (!node.IntoVariables.Any())
            {
                return;
            }

            walkThrough.Run(node, () =>
            {
                MultiAssignDetector.Monitor(
                    violations,
                    callback => AnalyzeFetch(node.IntoVariables, callback));
            });
        }

        private void AnalyzeFetch(IList<VariableReference> fetchedVars, Action<string, TSqlFragment> varAssignmentCallback)
        {
            foreach (var fetchVar in fetchedVars)
            {
                string varName = fetchVar.Name;

                // resetting value evaluations because we have no idea
                // what was fetched from cursor
                varRegistry.RegisterEvaluatedValue(varName, fetchVar.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, fetchVar));

                varAssignmentCallback(varName, fetchVar);
            }
        }
    }
}
