using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class DropWithNoCheckRule
    {
        private sealed class NestedDropsVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> suspiciousObjectDrops;
            private readonly HashSet<string> objectExistanceChecks;

            public NestedDropsVisitor(List<TSqlFragment> suspiciousObjectDrops, HashSet<string> objectExistanceChecks)
            {
                this.suspiciousObjectDrops = suspiciousObjectDrops;
                this.objectExistanceChecks = objectExistanceChecks;
            }

            // This covers DROP TABLE, PROC and other major object types
            public override void Visit(DropObjectsStatement node)
            {
                if (node.IsIfExists)
                {
                    return;
                }

                MarkNonSuspiciousIfChecked(node.Objects, ObjectTypeHandler.GetObjectTypeName(node));
            }

            public override void Visit(DropTypeStatement node)
            {
                if (node.IsIfExists)
                {
                    return;
                }

                MarkNonSuspiciousIfChecked(node, node.Name.GetFullName(), ObjectTypeHandler.GetObjectTypeName(node));
            }

            public override void Visit(DropSchemaStatement node)
            {
                if (node.IsIfExists)
                {
                    return;
                }

                MarkNonSuspiciousIfChecked(node, node.Schema.BaseIdentifier.Value, ObjectTypeHandler.GetObjectTypeName(node));
            }

            public override void Visit(AlterTableDropTableElementStatement node)
                => MarkNonSuspiciousIfChecked(node.AlterTableDropTableElements, node.SchemaObjectName);

            private bool IsUnChecked(string fullName, string objectType)
            {
                if (objectExistanceChecks.Contains(fullName))
                {
                    // there is a check with no type provided
                    return false;
                }

                if (string.IsNullOrEmpty(objectType))
                {
                    // unsupported object type
                    // returning `false` to prevent possible false-positive detection
                    return false;
                }

                if (objectExistanceChecks.Contains($"{objectType}:{fullName}"))
                {
                    // there is a check with type explicitly provided
                    return false;
                }

                // no pre-drop check
                return true;
            }

            private void MarkNonSuspiciousIfChecked(TSqlFragment node, string objectName, string objectType)
            {
                if (IsUnChecked(objectName, objectType))
                {
                    return;
                }

                // removing from suspicious if there is IF before drop
                // which checks all the dropped objects of given statement
                suspiciousObjectDrops.Remove(node);
            }

            private void MarkNonSuspiciousIfChecked(IList<SchemaObjectName> droppedObjects, string objectType)
            {
                for (int i = droppedObjects.Count - 1; i >= 0; i--)
                {
                    var item = droppedObjects[i];
                    if (!IsUnChecked(item.GetFullName(), objectType))
                    {
                        // removing from suspicious if there is IF before drop
                        // which checks all the dropped objects of given statement
                        suspiciousObjectDrops.Remove(item);
                    }
                }
            }

            private void MarkNonSuspiciousIfChecked(IList<AlterTableDropTableElement> droppedObjects, SchemaObjectName tableName)
            {
                string tableSchemaName = tableName.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                for (int i = droppedObjects.Count - 1; i >= 0; i--)
                {
                    var item = droppedObjects[i];

                    if (item.IsIfExists
                    || !ObjectTypeHandler.IsSupportedTableElementType(item.TableElementType))
                    {
                        continue;
                    }

                    // Constraints must be prepended with the table schema in OBJECT_ID call
                    var itemFullName = $"{tableSchemaName}.{item.Name.Value}";
                    var itemTypeName = ObjectTypeHandler.GetObjectTypeName(item.TableElementType);

                    if (!IsUnChecked(itemFullName, itemTypeName))
                    {
                        suspiciousObjectDrops.Remove(item);
                    }
                }
            }
        }
    }
}
