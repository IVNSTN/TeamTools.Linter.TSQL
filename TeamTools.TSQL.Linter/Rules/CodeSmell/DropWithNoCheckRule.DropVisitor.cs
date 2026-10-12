using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class DropWithNoCheckRule
    {
        private sealed class DropVisitor : TSqlFragmentVisitor
        {
            private static readonly string TableObjectType = ObjectTypeHandler.GetObjectTypeName(new CreateTableStatement());
            private static readonly string ConstraintObjectType = ObjectTypeHandler.GetObjectTypeName(TableElementType.Constraint);

            private List<TSqlFragment> drops;

            public DropVisitor() { }

            public List<TSqlFragment> SuspiciousDrops => drops;

            public bool SuspiciousDropsFound => SuspiciousDrops != null && SuspiciousDrops.Count > 0;

            private HashSet<string> LocalCreations { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Object creation can mean valid CREATE-DROP scenario.
            // CREATE PROC/FUNC/TRIGGER/VIEW/SCHEMA must be the first statement in a batch and should be the only statement
            // thus ignoring them - they dont seem to be valid object types for CREATE-DROP schenario
            // and their existence must be checked anyways. The rule works on per-batch basis.
            public override void Visit(CreateTableStatement node)
            {
                RegisterLocalCreation($"{TableObjectType}:{node.SchemaObjectName.GetFullName()}");
                RegisterTableConstraintsCreation(node.SchemaObjectName, node.Definition);
            }

            public override void Visit(AlterTableAddTableElementStatement node)
            {
                RegisterTableConstraintsCreation(node.SchemaObjectName, node.Definition);
            }

            // DROPs which may be illegal
            public override void Visit(DropObjectsStatement node)
            {
                if (!node.IsIfExists)
                {
                    RegisterDrop(node.Objects);
                }
            }

            public override void Visit(DropTypeStatement node)
            {
                if (!node.IsIfExists)
                {
                    RegisterDrop(node);
                }
            }

            public override void Visit(DropSchemaStatement node)
            {
                if (!node.IsIfExists)
                {
                    RegisterDrop(node);
                }
            }

            public override void ExplicitVisit(DropTableStatement node)
            {
                if (node.IsIfExists)
                {
                    return;
                }

                for (int i = node.Objects.Count - 1; i >= 0; i--)
                {
                    var item = node.Objects[i];
                    if (!IsJustCreated(item.GetFullName(), TableObjectType))
                    {
                        RegisterDrop(item);
                    }
                }
            }

            public override void Visit(AlterTableDropTableElementStatement node)
            {
                string tableSchemaName = node.SchemaObjectName.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                for (int i = node.AlterTableDropTableElements.Count - 1; i >= 0; i--)
                {
                    var item = node.AlterTableDropTableElements[i];

                    if (!item.IsIfExists
                    && ObjectTypeHandler.IsSupportedTableElementType(item.TableElementType)
                    && !IsJustCreated($"{tableSchemaName}.{item.Name.Value}", ConstraintObjectType))
                    {
                        RegisterDrop(item);
                    }
                }
            }

            private bool IsJustCreated(string objectIdentifier, string objectTypeName)
            {
                return !string.IsNullOrEmpty(objectIdentifier)
                    && !string.IsNullOrEmpty(objectTypeName)
                    && LocalCreations.Contains($"{objectTypeName}:{objectIdentifier}");
            }

            private void RegisterDrop(TSqlFragment node)
            {
                (drops ?? (drops = new List<TSqlFragment>())).Add(node);
            }

            private void RegisterDrop<T>(IList<T> nodes)
            where T : TSqlFragment
            {
                (drops ?? (drops = new List<TSqlFragment>())).AddRange(nodes);
            }

            private void RegisterLocalCreation(string objectIdentifier)
            {
                LocalCreations.Add(objectIdentifier);
            }

            private void RegisterTableConstraintsCreation(SchemaObjectName table, TableDefinition body)
            {
                if (body is null)
                {
                    // e.g. filetable or only table option switch
                    return;
                }

                string schema = table.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
                string prefix = $"{ConstraintObjectType}:{schema}";

                for (int i = body.TableConstraints.Count - 1; i >= 0; i--)
                {
                    var cstr = body.TableConstraints[i].ConstraintIdentifier;
                    if (cstr is null)
                    {
                        // impossible to identify noname constraint later
                        continue;
                    }

                    RegisterLocalCreation($"{prefix}.{cstr.Value}");
                }

                // a column may contain inline constraint definition
                for (int i = body.ColumnDefinitions.Count - 1; i >= 0; i--)
                {
                    var col = body.ColumnDefinitions[i];
                    for (int j = col.Constraints.Count - 1; j >= 0; j--)
                    {
                        var cstr = col.Constraints[j].ConstraintIdentifier;
                        if (cstr is null)
                        {
                            // impossible to identify noname constraint later
                            continue;
                        }

                        RegisterLocalCreation($"{prefix}.{cstr.Value}");
                    }

                    if (col.DefaultConstraint?.ConstraintIdentifier != null)
                    {
                        RegisterLocalCreation($"{prefix}.{col.DefaultConstraint.ConstraintIdentifier.Value}");
                    }
                }
            }
        }
    }
}
