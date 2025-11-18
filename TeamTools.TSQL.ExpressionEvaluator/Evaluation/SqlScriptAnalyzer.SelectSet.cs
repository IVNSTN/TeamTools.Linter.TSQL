using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// SELECT @var = value assignments processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(QuerySpecification node)
        {
            var setVarElements = ExtractSetVarElements(node.SelectElements);

            if (!setVarElements.Any())
            {
                return;
            }

            var setVarElementList = setVarElements.ToList();

            var evalQuery = new Action<QuerySpecification>(nd =>
                MultiAssignDetector.Monitor(
                    violations,
                    callback =>
                    {
                        AnalyzeQuerySpecification(setVarElementList, callback);

                        // FROM and WHERE might mean that no rows were processed
                        // and thus no assignments occured
                        // TODO : detect FROM VALUES and always true WHERE predicates
                        if (nd.FromClause != null || nd.WhereClause != null)
                        {
                            ResetAssignmentsAfterConditionalSelect(nd);
                        }
                    }));

            walkThrough.Run(node, evalQuery);
        }

        private static IEnumerable<SelectSetVariable> ExtractSetVarElements(IList<SelectElement> selectedItems)
        {
            int n = selectedItems.Count;
            for (int i = 0; i < n; i++)
            {
                // XML methods are not Expressions
                if (selectedItems[i] is SelectSetVariable setVar
                && setVar.Expression != null)
                {
                    yield return setVar;
                }
            }
        }

        private void AnalyzeQuerySpecification(List<SelectSetVariable> setVarElements, Action<string, TSqlFragment> callback)
        {
            int n = setVarElements.Count;
            for (int i = 0; i < n; i++)
            {
                var selVar = setVarElements[i];
                string varName = selVar.Variable.Name;
                this.ProcessVariableAssignment(varName, selVar.Expression, selVar.AssignmentKind);

                callback(varName, selVar);
            }
        }

        private void ResetAssignmentsAfterConditionalSelect(TSqlFragment node)
        {
            VarRegistry.ResetEvaluatedValuesAfterBlock(node.FirstTokenIndex, node.LastTokenIndex, new SqlValueSource(SqlValueSourceKind.Expression, node));
        }
    }
}
