using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableModificationDetector : TableFillDetector
    {
        public TableModificationDetector(Action<string, TriggerActionType, TSqlFragment> callback) : base(callback)
        {
        }

        public override void Visit(DeleteStatement node)
            => DetectModification(TriggerActionType.Delete, node.DeleteSpecification.Target, node.DeleteSpecification.FromClause);

        public override void Visit(UpdateStatement node)
            => DetectModification(TriggerActionType.Update, node.UpdateSpecification.Target, node.UpdateSpecification.FromClause);

        private void DetectModification(TriggerActionType action, TableReference target, FromClause from)
        {
            var tbl = new TableReferenceDetector(FilledTables, tblName => TableReferenceDetected(tblName, action, target));
            TSqlFragment link = AliasToTarget(target, from);
            link.Accept(tbl);
        }
    }
}
