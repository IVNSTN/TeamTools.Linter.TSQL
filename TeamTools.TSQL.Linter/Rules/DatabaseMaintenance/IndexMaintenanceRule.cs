using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0729", "INDEX_MAINTENANCE")]
    [IndexRule]
    internal sealed class IndexMaintenanceRule : AbstractRule
    {
        private readonly IndexMaintenanceVisitor indexCreateDetector;

        public IndexMaintenanceRule() : base()
        {
            indexCreateDetector = new IndexMaintenanceVisitor(ViolationHandler);
        }

        public override void Visit(ProcedureStatementBody node) => node.StatementList?.Accept(indexCreateDetector);

        public override void Visit(TriggerStatementBody node) => node.StatementList?.Accept(indexCreateDetector);

        public override void Visit(AlterIndexStatement node) => HandleNodeError(node);

        public override void Visit(DropIndexStatement node) => HandleNodeError(node);

        private class IndexMaintenanceVisitor : VisitorWithCallback
        {
            public IndexMaintenanceVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

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
