using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// EXEC proc analysis to detect all possible variable assignments:
    /// EXEC @res = proc and -- return value
    /// EXEC proc @param = @var OUT -- output parameters.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // EXEC @res = <proc> + OUT params
        public override void Visit(ExecuteSpecification node)
        {
            walkThrough.Run(node, () =>
                MultiAssignDetector.Monitor(
                violations,
                callback => AnalyzeExecute(node, callback)));
        }

        private void AnalyzeExecute(ExecuteSpecification node, Action<string, TSqlFragment> varAssignmentCallback)
        {
            // EXEC @res = ...
            if (node.Variable != null)
            {
                string varName = node.Variable.Name;

                varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, node.Variable));

                varAssignmentCallback(varName, node.Variable);
            }

            // OUT params
            var outVars = node.ExecutableEntity.Parameters
                .Where(p => p.IsOutput && p.ParameterValue != null)
                .Select(p => p.ParameterValue)
                .OfType<VariableReference>();

            foreach (var outVar in outVars)
            {
                string varName = outVar.Name;
                // resetting value evaluations because we have no idea
                // what a proc would put in OUTPUT parameter
                varRegistry.RegisterEvaluatedValue(varName, outVar.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, outVar));

                varAssignmentCallback(varName, outVar);
            }
        }
    }
}
