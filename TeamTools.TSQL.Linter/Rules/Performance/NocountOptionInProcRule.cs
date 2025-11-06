using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0144", "PROC_SET_NOCOUNT")]
    internal sealed class NocountOptionInProcRule : AbstractRule
    {
        public NocountOptionInProcRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            var nocountVisitor = new SetOptionsVisitor();
            node.AcceptChildren(nocountVisitor);

            if (null == node.StatementList)
            {
                // considering empty proc as ok
                return;
            }

            if (node.StatementList.Statements[0] is BeginEndAtomicBlockStatement)
            {
                // in atomic NOCOUNT is always ON
                return;
            }

            if (nocountVisitor.DetectedOptions.ContainsKey(SetOptions.NoCount.ToString()))
            {
                return;
            }

            HandleNodeError(GetFirstStatement(node.StatementList));
        }

        private static TSqlFragment GetFirstStatement(TSqlFragment node)
        {
            if (node is BeginEndBlockStatement be && (be.StatementList?.Statements?.Count ?? 0) > 0)
            {
                return GetFirstStatement(be.StatementList?.Statements[0]);
            }

            if (node is StatementList sl && sl.Statements.Count > 0)
            {
                return GetFirstStatement(sl.Statements[0]);
            }

            return node;
        }
    }
}
