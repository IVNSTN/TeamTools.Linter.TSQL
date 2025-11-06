using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    // TODO : support aliases, cte
    internal class TableFillDetector : TSqlFragmentVisitor
    {
        public TableFillDetector(Action<string, TriggerActionType, TSqlFragment> callback)
        {
            Callback = callback;
        }

        public IDictionary<string, TSqlFragment> FilledTables { get; } = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

        protected Action<string, TriggerActionType, TSqlFragment> Callback { get; }

        public override void Visit(TriggerStatementBody node)
        {
            if (node.TriggerActions.Count > 1 || node.TriggerActions[0].TriggerActionType == TriggerActionType.Update)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Inserted, node.TriggerObject);
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Deleted, node.TriggerObject);
            }
            else if (node.TriggerActions[0].TriggerActionType == TriggerActionType.Delete)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Deleted, node.TriggerObject);
            }
            else if (node.TriggerActions[0].TriggerActionType == TriggerActionType.Insert)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Inserted, node.TriggerObject);
            }

            FilledTables.TryAdd(node.TriggerObject.Name.GetFullName(), node.TriggerObject);
        }

        public override void Visit(InsertStatement node) => VisitInsert(node.InsertSpecification);

        public override void Visit(OutputIntoClause node) => VisitInsert(node.IntoTable);

        public override void Visit(MergeStatement node)
        {
            var tbl = new TableReferenceDetector(FilledTables, tblName =>
            {
                // TODO : get actual actions from the merge statement
                TableReferenceDetected(tblName, TriggerActionType.Insert, node);
                TableReferenceDetected(tblName, TriggerActionType.Update, node);
                TableReferenceDetected(tblName, TriggerActionType.Delete, node);
            });
            node.MergeSpecification.Target.Accept(tbl);
        }

        protected void TableReferenceDetected(string tableName, TriggerActionType action, TSqlFragment node)
        {
            Callback?.Invoke(tableName, action, node);
        }

        protected TSqlFragment AliasToTarget(TSqlFragment alias, FromClause from)
        {
            // TODO : better support aliases, cte, derived tables
            var realTarget = alias;

            if (from == null)
            {
                return realTarget;
            }

            // looks like an alias;
            if (realTarget is NamedTableReference nm && nm.Alias == null && nm.SchemaObject.SchemaIdentifier == null)
            {
                string aliasName = nm.SchemaObject.BaseIdentifier.Value;

                foreach (var t in from.TableReferences)
                {
                    realTarget = IfMatchedAliasSourceThen(realTarget, aliasName, t);
                }
            }

            return realTarget;
        }

        private void VisitInsert(TSqlFragment node)
        {
            var tbl = new TableReferenceDetector(FilledTables, tblName => TableReferenceDetected(tblName, TriggerActionType.Insert, node));
            node.Accept(tbl);
        }

        private TSqlFragment IfMatchedAliasSourceThen(TSqlFragment original, string aliasName, TableReference tblRef)
        {
            var realTarget = original;

            if (tblRef is JoinTableReference j)
            {
                realTarget = IfMatchedAliasSourceThen(realTarget, aliasName, j.FirstTableReference);
                realTarget = IfMatchedAliasSourceThen(realTarget, aliasName, j.SecondTableReference);
            }
            else if (tblRef is NamedTableReference nm)
            {
                if (nm.Alias != null && aliasName.Equals(nm.Alias.Value, StringComparison.OrdinalIgnoreCase))
                {
                    realTarget = nm;
                }
            }

            return realTarget;
        }
    }
}
