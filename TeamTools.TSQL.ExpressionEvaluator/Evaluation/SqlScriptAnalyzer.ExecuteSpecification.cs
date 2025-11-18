using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// EXEC proc analysis to detect all possible variable assignments:
    /// EXEC @res = proc and -- return value
    /// EXEC proc @param = @var OUT -- output parameters.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<ExecuteSpecification> execEval;

        // EXEC @res = <proc> + OUT params
        public override void Visit(ExecuteSpecification node)
        {
            walkThrough.Run(node, execEval ?? (execEval = new Action<ExecuteSpecification>(ExecEval)));
        }

        private void ExecEval(ExecuteSpecification exec)
        {
            MultiAssignDetector.Monitor(
                violations,
                callback => AnalyzeExecute(exec, callback));
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

            // Local variables get modified by being passed as OUTPUT params to other proc
            int n = node.ExecutableEntity.Parameters.Count;
            for (int i = 0; i < n; i++)
            {
                var p = node.ExecutableEntity.Parameters[i];
                if (p.IsOutput && p.ParameterValue != null
                && p.ParameterValue is VariableReference outVar)
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
}
