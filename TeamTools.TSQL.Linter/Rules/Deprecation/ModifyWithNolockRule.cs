using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0819", "MODIFY_WITH_NOLOCK")]
    internal sealed class ModifyWithNolockRule : AbstractRule
    {
        public ModifyWithNolockRule() : base()
        {
        }

        public override void Visit(DataModificationStatement node)
        {
            if (!(node is UpdateStatement || node is DeleteStatement))
            {
                // It is deprecated for UPDATE and DELETE only
                // Docs: https://learn.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table?view=sql-server-ver16#nolock
                return;
            }

            var hintVisitor = new NolockVisitor(ViolationHandler);
            node.Accept(hintVisitor);
        }

        private class NolockVisitor : VisitorWithCallback
        {
            public NolockVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(TableHint node)
            {
                if (node.HintKind == TableHintKind.NoLock || node.HintKind == TableHintKind.ReadUncommitted)
                {
                    Callback.Invoke(node);
                }
            }
        }
    }
}
