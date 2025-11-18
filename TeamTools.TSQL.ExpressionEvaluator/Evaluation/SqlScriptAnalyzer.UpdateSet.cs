using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// UPDATE SET @var = value assignments processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(UpdateSpecification node)
        {
            var setVarElements = ExtractSetVarClauses(node.SetClauses).ToList();

            if (setVarElements.Count == 0)
            {
                return;
            }

            var evalUpd = new Action<UpdateSpecification>(upd =>
                MultiAssignDetector.Monitor(
                    violations,
                    callback =>
                    {
                        AnalyzeQuerySpecification(setVarElements, callback);

                        // FROM and WHERE might mean that no rows were processed
                        // and thus no assignments occured
                        // TODO : detect FROM VALUES and always true WHERE predicates
                        if (upd.FromClause != null || upd.WhereClause != null)
                        {
                            ResetAssignmentsAfterConditionalSelect(upd);
                        }
                    }));

            walkThrough.Run(node, evalUpd);
        }

        private static IEnumerable<AssignmentSetClause> ExtractSetVarClauses(IList<SetClause> clauses)
        {
            int n = clauses.Count;
            for (int i = 0; i < n; i++)
            {
                if (clauses[i] is AssignmentSetClause asgn && asgn.Variable != null)
                {
                    yield return asgn;
                }
            }
        }

        private void AnalyzeQuerySpecification(List<AssignmentSetClause> setVarElements, Action<string, TSqlFragment> callback)
        {
            int n = setVarElements.Count;
            for (int i = 0; i < n; i++)
            {
                var selVar = setVarElements[i];
                string varName = selVar.Variable.Name;
                this.ProcessVariableAssignment(varName, selVar.NewValue, selVar.AssignmentKind);

                callback(varName, selVar);
            }
        }
    }
}
