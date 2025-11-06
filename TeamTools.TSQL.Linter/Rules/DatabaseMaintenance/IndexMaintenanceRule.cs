using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0729", "INDEX_MAINTENANCE")]
    [IndexRule]
    internal sealed class IndexMaintenanceRule : AbstractRule
    {
        public IndexMaintenanceRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node) => DetectIndexMaintenance(node.StatementList);

        public override void Visit(TriggerStatementBody node) => DetectIndexMaintenance(node.StatementList);

        // Searching in the script root
        public override void Visit(TSqlScript node)
        {
            foreach (var batch in node.Batches)
            {
                foreach (var stmt in batch.Statements.SelectMany(s => ExtractStatement(s)))
                {
                    if (stmt is AlterIndexStatement || stmt is DropIndexStatement)
                    {
                        HandleNodeError(stmt);
                    }
                }
            }
        }

        private IEnumerable<TSqlStatement> ExtractStatement(TSqlStatement stmt)
        {
            if (stmt is BeginEndBlockStatement be)
            {
                foreach (var s in be.StatementList.Statements.SelectMany(ss => ExtractStatement(ss)))
                {
                    yield return s;
                }

                yield break;
            }

            yield return stmt;
        }

        private void DetectIndexMaintenance(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // external proc
                return;
            }

            body.Accept(new IndexMaintenanceVisitor(HandleNodeError));
        }

        private class IndexMaintenanceVisitor : VisitorWithCallback
        {
            public IndexMaintenanceVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(AlterIndexStatement node) => Callback(node);

            public override void Visit(DropIndexStatement node) => Callback(node);

            public override void Visit(CreateIndexStatement node)
            {
                if (node.OnName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    // creating indices on # is fine
                    return;
                }

                Callback(node);
            }
        }
    }
}
