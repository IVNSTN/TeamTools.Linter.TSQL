using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Registers @@PROCID global var for programmability body to be reused in evaluation.
    /// </summary>
    // TODO : very similar to SqlScriptAnalyzer.EvaluateProcId.cs
    public partial class VariableDeclarationVisitor
    {
        public override void Visit(ProcedureStatementBody node) => RegisterProcIdVar(node.StatementList);

        public override void Visit(TriggerStatementBody node) => RegisterProcIdVar(node.StatementList);

        public override void Visit(FunctionStatementBody node) => RegisterProcIdVar(node.StatementList);

        private void RegisterProcIdVar(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // no body - no vars
                return;
            }

            varRegistry.RegisterVariable("@@PROCID", typeResolver.ResolveType("dbo.INT"));
        }
    }
}
