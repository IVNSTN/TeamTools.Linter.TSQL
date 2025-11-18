using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Registers @@PROCID global var for programmability body to be reused in evaluation.
    /// </summary>
    // TODO : very similar to SqlScriptAnalyzer.EvaluateProcId.cs
    public partial class VariableDeclarationVisitor
    {
        public override void Visit(ProcedureStatementBody node)
        {
            RegisterProcIdVar(node.StatementList);
            RegisterReturnValue(node.StatementList, TSqlDomainAttributes.Types.Int);
        }

        public override void Visit(FunctionStatementBody node)
        {
            RegisterProcIdVar(node.StatementList);

            if (node.ReturnType is ScalarFunctionReturnType ret)
            {
                RegisterReturnValue(node.StatementList, ret.DataType);
            }
        }

        public override void Visit(TriggerStatementBody node) => RegisterProcIdVar(node.StatementList);

        private void RegisterProcIdVar(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // no body - no vars
                return;
            }

            const string procIdVar = "@@PROCID";
            varRegistry.RegisterVariable(procIdVar, typeResolver.ResolveType(TSqlDomainAttributes.Types.Int));
        }
    }
}
