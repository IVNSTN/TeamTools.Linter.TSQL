using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// SELECT @var = value assignments processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(QuerySpecification node)
        {
            var setVarElements = node.SelectElements
                .OfType<SelectSetVariable>()
                // XML methods are not Expressions
                .Where(s => s.Expression != null)
                .ToList();

            if (setVarElements.Count == 0)
            {
                return;
            }

            walkThrough.Run(node, () =>
                MultiAssignDetector.Monitor(
                    violations,
                    callback =>
                    {
                        AnalyzeQuerySpecification(setVarElements, callback);

                        // FROM and WHERE might mean that no rows were processed
                        // and thus no assignments occured
                        // TODO : detect FROM VALUES and always true WHERE predicates
                        if (node.FromClause != null || node.WhereClause != null)
                        {
                            ResetAssignmentsAfterConditionalSelect(node);
                        }
                    }));
        }

        private void AnalyzeQuerySpecification(IList<SelectSetVariable> setVarElements, Action<string, TSqlFragment> callback)
        {
            foreach (var selVar in setVarElements)
            {
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
