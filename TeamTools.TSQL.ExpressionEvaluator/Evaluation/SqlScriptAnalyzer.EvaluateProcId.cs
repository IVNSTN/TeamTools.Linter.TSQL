using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Create PROCEDURE and TRIGGER processing for @@PROCID magic.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(ProcedureStatementBody node) => RegisterProcId(node.ProcedureReference.Name, node.StatementList);

        public override void Visit(TriggerStatementBody node) => RegisterProcId(node.Name, node.StatementList);

        public override void Visit(FunctionStatementBody node) => RegisterProcId(node.Name, node.StatementList);

        private void RegisterProcId(SchemaObjectName name, StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // no body - no need to do anything
                return;
            }

            this.EvaluateProcId(name, body, body.FirstTokenIndex, body.LastTokenIndex);
        }
    }
}
