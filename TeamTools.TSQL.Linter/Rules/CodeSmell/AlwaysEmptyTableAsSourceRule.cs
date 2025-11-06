using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0190", "ALWAYS_EMPTY_SOURCE")]
    internal sealed class AlwaysEmptyTableAsSourceRule : AbstractRule
    {
        // very similar to VariableNotAssignedBeforeUsageRule
        public AlwaysEmptyTableAsSourceRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var validator = new TableUsageValidator();
            node.AcceptChildren(validator);

            foreach (var badRef in validator.AccessingNeverFilledTables)
            {
                HandleNodeError(badRef.Value, badRef.Key);
            }
        }

        private class TableUsageValidator : TSqlFragmentVisitor
        {
            // key=table name, value=true if table fill found
            private readonly IDictionary<string, bool> tables = new SortedDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            private readonly IDictionary<string, TSqlFragment> badRefs = new SortedDictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public IDictionary<string, TSqlFragment> AccessingNeverFilledTables => badRefs;

            public override void Visit(CreateTableStatement node)
                => RegisterTableDeclaration(node.SchemaObjectName.GetFullName());

            public override void Visit(DeclareTableVariableBody node)
                => RegisterTableDeclaration(node.VariableName?.Value);

            // possible table-UDT var
            public override void Visit(DeclareVariableElement node)
            {
                if (node is ProcedureParameter)
                {
                    // params are filled outside
                    return;
                }

                RegisterTableDeclaration(node.VariableName.Value);
            }

            // internal INSERTED, DELETED tables
            public override void Visit(TriggerStatementBody node)
            {
                RegisterTableDeclaration(TSqlDomainAttributes.TriggerSystemTables.Inserted);
                RegisterTableDeclaration(TSqlDomainAttributes.TriggerSystemTables.Deleted);

                if (node.TriggerActions.Count > 1 || node.TriggerActions[0].TriggerActionType == TriggerActionType.Update)
                {
                    tables[TSqlDomainAttributes.TriggerSystemTables.Inserted] = true;
                    tables[TSqlDomainAttributes.TriggerSystemTables.Deleted] = true;
                }
                else if (node.TriggerActions[0].TriggerActionType == TriggerActionType.Delete)
                {
                    tables[TSqlDomainAttributes.TriggerSystemTables.Deleted] = true;
                }
                else if (node.TriggerActions[0].TriggerActionType == TriggerActionType.Insert)
                {
                    tables[TSqlDomainAttributes.TriggerSystemTables.Inserted] = true;
                }
            }

            public override void Visit(TableReference node)
            {
                string tableName = GetTargetTableName(node);

                if (string.IsNullOrEmpty(tableName) || badRefs.ContainsKey(tableName))
                {
                    return;
                }

                if (!tables.ContainsKey(tableName) || tables[tableName] == true)
                {
                    return;
                }

                badRefs.Add(tableName, node);
            }

            // filling table with SELECT INTO
            public override void Visit(SelectStatement node) => MarkTableFilled(node.Into?.GetFullName());

            // filling table with INSERT
            public override void Visit(InsertSpecification node) => MarkTableFilled(node.Target);

            // filling table with OUTPUT INTO
            public override void Visit(OutputIntoClause node) => MarkTableFilled(node.IntoTable);

            // filling table with MERGE
            public override void Visit(MergeSpecification node) => MarkTableFilled(node.Target);

            private static string GetTargetTableName(TableReference target)
            {
                if (target is null)
                {
                    return default;
                }

                if (target is VariableTableReference tblVar)
                {
                    return tblVar.Variable.Name;
                }

                if (target is NamedTableReference tbl)
                {
                    return tbl.SchemaObject.GetFullName();
                }

                return default;
            }

            private void MarkTableFilled(TableReference table) => MarkTableFilled(GetTargetTableName(table));

            private void MarkTableFilled(string tableName)
            {
                if (!string.IsNullOrEmpty(tableName) && tables.ContainsKey(tableName))
                {
                    tables[tableName] = true;
                }
            }

            private void RegisterTableDeclaration(string tableName)
            {
                if (!string.IsNullOrEmpty(tableName) && !tables.ContainsKey(tableName))
                {
                    tables.Add(tableName, false);
                }
            }
        }
    }
}
