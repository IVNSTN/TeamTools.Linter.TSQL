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
            var act = node.TriggerActions[0];
            if (node.TriggerActions.Count > 1 || act.TriggerActionType == TriggerActionType.Update)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Inserted, node.TriggerObject);
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Deleted, node.TriggerObject);
            }
            else if (act.TriggerActionType == TriggerActionType.Delete)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Deleted, node.TriggerObject);
            }
            else if (act.TriggerActionType == TriggerActionType.Insert)
            {
                FilledTables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Inserted, node.TriggerObject);
            }

            FilledTables.TryAdd(node.TriggerObject.Name.GetFullName(), node.TriggerObject);
        }

        public override void Visit(InsertStatement node) => VisitInsert(node.InsertSpecification);

        public override void Visit(OutputIntoClause node) => VisitInsert(node.IntoTable);

        public override void Visit(MergeStatement node)
        {
            var tbl = new TableReferenceDetector(FilledTables, tblName => RegisterMergeActions(node.MergeSpecification, tblName));
            node.MergeSpecification.Target.Accept(tbl);
        }

        protected void TableReferenceDetected(string tableName, TriggerActionType action, TSqlFragment node)
        {
            Callback?.Invoke(tableName, action, node);
        }

        protected TSqlFragment AliasToTarget(TSqlFragment alias, FromClause from)
        {
            var realTarget = alias;

            if (from is null)
            {
                return realTarget;
            }

            // looks like an alias;
            if (realTarget is NamedTableReference nm && nm.Alias is null && nm.SchemaObject.SchemaIdentifier is null)
            {
                string aliasName = nm.SchemaObject.BaseIdentifier.Value;

                for (int i = from.TableReferences.Count - 1; i >= 0; i--)
                {
                    realTarget = IfMatchedAliasSourceThen(realTarget, aliasName, from.TableReferences[i]);
                }
            }

            return realTarget;
        }

        private static TriggerActionType ConvertMergeActionToModificationType(MergeAction act)
        {
            if (act is InsertMergeAction)
            {
                return TriggerActionType.Insert;
            }

            if (act is UpdateMergeAction)
            {
                return TriggerActionType.Update;
            }

            return TriggerActionType.Delete;
        }

        private static TSqlFragment IfMatchedAliasSourceThen(TSqlFragment original, string aliasName, TableReference tblRef)
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

        private void VisitInsert(TSqlFragment node)
        {
            var tbl = new TableReferenceDetector(FilledTables, tblName => TableReferenceDetected(tblName, TriggerActionType.Insert, node));
            node.Accept(tbl);
        }

        private void RegisterMergeActions(MergeSpecification node, string tblName)
        {
            for (int i = node.ActionClauses.Count - 1; i >= 0; i--)
            {
                TableReferenceDetected(tblName, ConvertMergeActionToModificationType(node.ActionClauses[i].Action), node);
            }
        }
    }
}
