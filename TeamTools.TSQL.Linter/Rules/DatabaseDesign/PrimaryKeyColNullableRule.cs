using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0184", "PK_COLUMN_NOT_NULL")]
    internal sealed class PrimaryKeyColNullableRule : AbstractRule
    {
        public PrimaryKeyColNullableRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var tables = new Dictionary<string, IList<ColumnDefinition>>(StringComparer.OrdinalIgnoreCase);
            var constraints = new Dictionary<string, IList<ConstraintDefinition>>(StringComparer.OrdinalIgnoreCase);
            var tableVisitor = new CreateTableVisitor(tables, constraints);
            node.Accept(tableVisitor);

            ValidateTableConstraints(tables, constraints);
        }

        private void ValidatePrimaryKeyColumns(IList<ColumnWithSortOrder> constraintColumns, IList<ColumnDefinition> tableColumns)
        {
            foreach (var col in constraintColumns)
            {
                foreach (var tblCol in tableColumns)
                {
                    if (string.Equals(tblCol.ColumnIdentifier.Value, col.Column.MultiPartIdentifier.Identifiers.LastOrDefault()?.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        var nullConstraint = tblCol.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault();
                        if (nullConstraint == null || nullConstraint.Nullable)
                        {
                            HandleNodeError(tblCol);
                        }
                    }
                }
            }
        }

        private void ValidateTableConstraints(IDictionary<string, IList<ColumnDefinition>> tables, IDictionary<string, IList<ConstraintDefinition>> constraints)
        {
            foreach (var tbl in constraints.Keys.Where(tbl => tables.ContainsKey(tbl)))
            {
                foreach (var cs in constraints[tbl])
                {
                    if (!(cs is UniqueConstraintDefinition pk) || !pk.IsPrimaryKey)
                    {
                        continue;
                    }

                    ValidatePrimaryKeyColumns(pk.Columns, tables[tbl]);
                }
            }
        }

        private class CreateTableVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, IList<ColumnDefinition>> tables = null;
            private readonly IDictionary<string, IList<ConstraintDefinition>> constraints = null;

            public CreateTableVisitor(IDictionary<string, IList<ColumnDefinition>> tables, IDictionary<string, IList<ConstraintDefinition>> constraints)
            {
                this.tables = tables;
                this.constraints = constraints;
            }

            public override void Visit(DeclareTableVariableBody node)
            {
                // in inline-table function output definition has no name
                string tableName = node.VariableName?.Value ?? "result";

                tables.TryAdd(tableName, new List<ColumnDefinition>(node.Definition.ColumnDefinitions));
                constraints.TryAdd(tableName, new List<ConstraintDefinition>(node.Definition.TableConstraints.OfType<UniqueConstraintDefinition>()));

                foreach (var cs in GetConstraintsFromColumns(node.Definition.ColumnDefinitions))
                {
                    constraints[tableName].Add(cs);
                }
            }

            public override void Visit(CreateTableStatement node)
            {
                if ((node.Definition?.ColumnDefinitions.Count ?? 0) == 0)
                {
                    // FILETABLE
                    return;
                }

                string tableName = node.SchemaObjectName.GetFullName();

                tables.TryAdd(tableName, new List<ColumnDefinition>(node.Definition.ColumnDefinitions));
                constraints.TryAdd(tableName, new List<ConstraintDefinition>(node.Definition.TableConstraints.OfType<UniqueConstraintDefinition>()));

                foreach (var cs in GetConstraintsFromColumns(node.Definition.ColumnDefinitions))
                {
                    constraints[tableName].Add(cs);
                }
            }

            public override void Visit(AlterTableStatement node)
            {
                string tableName = node.SchemaObjectName.GetFullName();
                var primaryKeyVisitor = new AddPKConstraintVisitor(tableName, constraints);

                node.AcceptChildren(primaryKeyVisitor);
            }

            private IEnumerable<UniqueConstraintDefinition> GetConstraintsFromColumns(IList<ColumnDefinition> columns)
            {
                IEnumerable<KeyValuePair<ColumnDefinition, UniqueConstraintDefinition>> constraints =
                    from col in columns
                    from cs in col.Constraints
                    where cs is UniqueConstraintDefinition
                    select new KeyValuePair<ColumnDefinition, UniqueConstraintDefinition>(col, cs as UniqueConstraintDefinition);

                foreach (var cs in constraints)
                {
                    // inline constraint
                    // TODO : rethink
                    if (cs.Value.Columns.Count == 0)
                    {
                        var colRef = new ColumnWithSortOrder
                        {
                            Column = new ColumnReferenceExpression
                            {
                                MultiPartIdentifier = new MultiPartIdentifier(),
                            },
                        };

                        colRef.Column.MultiPartIdentifier.Identifiers.Add(
                            new Identifier
                            {
                                Value = cs.Key.ColumnIdentifier.Value,
                            });

                        cs.Value.Columns.Add(colRef);
                    }

                    yield return cs.Value;
                }
            }
        }

        private class AddPKConstraintVisitor : TSqlFragmentVisitor
        {
            private readonly string tableName;
            private readonly IDictionary<string, IList<ConstraintDefinition>> constraints;

            public AddPKConstraintVisitor(string tableName, IDictionary<string, IList<ConstraintDefinition>> constraints)
            {
                this.tableName = tableName;
                this.constraints = constraints;
            }

            public override void Visit(UniqueConstraintDefinition node)
            {
                if (!node.IsPrimaryKey)
                {
                    return;
                }

                if (!constraints.ContainsKey(tableName))
                {
                    constraints.Add(tableName, new List<ConstraintDefinition>());
                }

                constraints[tableName].Add(node);
            }
        }
    }
}
