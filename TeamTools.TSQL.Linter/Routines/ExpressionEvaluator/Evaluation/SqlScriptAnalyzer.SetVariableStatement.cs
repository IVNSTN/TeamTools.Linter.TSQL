using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// SET @var = value processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(SetVariableStatement node)
        {
            walkThrough.Run(node, () =>
            {
                this.ProcessVariableAssignment(node.Variable.Name, node.Expression, node.AssignmentKind);
            });
        }
    }
}
