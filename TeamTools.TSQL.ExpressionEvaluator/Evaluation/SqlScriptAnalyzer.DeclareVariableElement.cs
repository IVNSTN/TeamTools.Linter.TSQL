using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Assignments in DECLARE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<DeclareVariableElement> declareEval;

        public override void Visit(DeclareVariableElement node)
        {
            walkThrough.Run(node, declareEval ?? (declareEval = new Action<DeclareVariableElement>(AnalyzeVarAssignmentInDeclare)));
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

            if (node.Value is null || node.Value.ExtractScalarExpression() is NullLiteral)
            {
                // TODO : what about never initialized variables?
                this.VarRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Expression, node));
            }
            else
            {
                this.ProcessVariableAssignment(varName, node.Value, AssignmentKind.Equals);
            }
        }
    }
}
