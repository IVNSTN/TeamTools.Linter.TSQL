using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// SET @var = value processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<SetVariableStatement> evalSetVar;

        public override void Visit(SetVariableStatement node)
        {
            walkThrough.Run(node, evalSetVar ?? (evalSetVar = new Action<SetVariableStatement>(EvalSetVar)));
        }

        private void EvalSetVar(SetVariableStatement setVar)
        {
            this.ProcessVariableAssignment(setVar.Variable.Name, setVar.Expression, setVar.AssignmentKind);
        }
    }
}
