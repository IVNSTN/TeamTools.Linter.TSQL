using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Assignments in DECLARE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(DeclareVariableElement node)
        {
            walkThrough.Run(node, () => AnalyzeVarAssignmentInDeclare(node));
        }

        private void AnalyzeVarAssignmentInDeclare(DeclareVariableElement node)
        {
            string varName = node.VariableName?.Value;

            if (string.IsNullOrEmpty(varName))
            {
                return;
            }

            if (node is ProcedureParameter)
            {
                // params come from outside, no evaluation is possible
                varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, node));

                return;
            }

            // TODO : support (SELECT NULL), ((NULL))
            if (node.Value is null || node.Value is NullLiteral)
            {
                return;
            }

            this.ProcessVariableAssignment(varName, node.Value, AssignmentKind.Equals);
        }
    }
}
