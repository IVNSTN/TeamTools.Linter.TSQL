using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0977", "DROPPING_WITH_NO_CHECK")]
    internal sealed class DropWithNoCheckRule : AbstractRule
    {
        public DropWithNoCheckRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var dropVisitor = new DropVisitor();
            node.AcceptChildren(dropVisitor);

            if (!dropVisitor.SuspiciousDropsFound)
            {
                return;
            }

            var preceidingConditionVisitor = new ConditionalDropVisitor(dropVisitor.SuspiciousDrops);
            node.AcceptChildren(preceidingConditionVisitor);

            int n = dropVisitor.SuspiciousDrops.Count;
            for (int i = 0; i < n; i++)
            {
                HandleNodeError(dropVisitor.SuspiciousDrops[i]);
            }
        }

        private sealed class DropVisitor : TSqlFragmentVisitor
        {
            private List<TSqlFragment> drops;

            public DropVisitor() { }

            public List<TSqlFragment> SuspiciousDrops => drops;

            public bool SuspiciousDropsFound => SuspiciousDrops != null && SuspiciousDrops.Count > 0;

            public override void Visit(DropObjectsStatement node)
            {
                if (!node.IsIfExists)
                {
                    RegisterDrop(node);
                }
            }

            public override void Visit(DropTypeStatement node)
            {
                if (!node.IsIfExists)
                {
                    RegisterDrop(node);
                }
            }

            private void RegisterDrop(TSqlFragment node)
            {
                (drops ?? (drops = new List<TSqlFragment>())).Add(node);
            }
        }

        private sealed class ConditionalDropVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> suspiciousDrops;

            public ConditionalDropVisitor(List<TSqlFragment> suspiciousDrops)
            {
                this.suspiciousDrops = suspiciousDrops;
            }

            public override void Visit(IfStatement node)
            {
                var existanceChecks = new HashSet<string>(ExtractObjectExistanceChecks(node.Predicate, false), StringComparer.OrdinalIgnoreCase);

                if (existanceChecks.Count == 0)
                {
                    return;
                }

                var nestedDropsVisitor = new NestedDropsVisitor(suspiciousDrops, existanceChecks);
                node.ThenStatement.Accept(nestedDropsVisitor);
            }

            private IEnumerable<string> ExtractObjectExistanceChecks(BooleanExpression node, bool negated)
            {
                if (node is BooleanParenthesisExpression pe)
                {
                    return ExtractObjectExistanceChecks(pe.Expression, negated);
                }

                if (node is BooleanBinaryExpression bin)
                {
                    if ((negated && bin.BinaryExpressionType == BooleanBinaryExpressionType.Or)
                    || ((!negated) && bin.BinaryExpressionType == BooleanBinaryExpressionType.And))
                    {
                        // or is a strange logic thus ignoring such cases
                        return ExtractObjectExistanceChecks(bin.FirstExpression, negated)
                            .Union(ExtractObjectExistanceChecks(bin.SecondExpression, negated));
                    }
                    else
                    {
                        return Enumerable.Empty<string>();
                    }
                }

                if (node is BooleanNotExpression ne)
                {
                    return ExtractObjectExistanceChecks(ne.Expression, !negated);
                }

                if (node is BooleanIsNullExpression isnull
                && (isnull.IsNot ^ negated))
                {
                    return Enumerable.Repeat<string>(ExtractObjectExistanceCheck(isnull.Expression), 1);
                }

                return Enumerable.Empty<string>();
            }

            private string ExtractObjectExistanceCheck(ScalarExpression node)
            {
                if (node is ParenthesisExpression pe)
                {
                    return ExtractObjectExistanceCheck(pe.Expression);
                }

                if (node is FunctionCall fn)
                {
                    // TODO : also check object type compatibility
                    if (fn.FunctionName.Value.Equals("OBJECT_ID", StringComparison.OrdinalIgnoreCase))
                    {
                        if (fn.Parameters.Count > 0 && fn.Parameters[0] is StringLiteral str)
                        {
                            if (!str.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                            && !str.Value.StartsWith(TSqlDomainAttributes.VariablePrefix)
                            && !str.Value.Contains(TSqlDomainAttributes.NamePartSeparator))
                            {
                                return TSqlDomainAttributes.DefaultSchemaPrefix + str.Value;
                            }
                            else
                            {
                                return str.Value;
                            }
                        }
                    }
                }

                return default;
            }
        }

        private sealed class NestedDropsVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> suspiciousObjectDrops;
            private readonly HashSet<string> objectExistanceChecks;

            public NestedDropsVisitor(List<TSqlFragment> suspiciousObjectDrops, HashSet<string> objectExistanceChecks)
            {
                this.suspiciousObjectDrops = suspiciousObjectDrops;
                this.objectExistanceChecks = objectExistanceChecks;
            }

            public override void Visit(DropObjectsStatement node) => MarkNonSuspiciousIfChecked(node, node.Objects);

            public override void Visit(DropTypeStatement node) => MarkNonSuspiciousIfChecked(node, new List<SchemaObjectName> { node.Name });

            private bool IsUnChecked(SchemaObjectName name)
            {
                return !objectExistanceChecks.Contains(name.GetFullName());
            }

            private void MarkNonSuspiciousIfChecked(TSqlFragment node, SchemaObjectName droppedObject)
            {
                if (IsUnChecked(droppedObject))
                {
                    return;
                }

                // removing from suspicious if there is IF before drop
                suspiciousObjectDrops.Remove(node);
            }

            private void MarkNonSuspiciousIfChecked(TSqlFragment node, IList<SchemaObjectName> droppedObjects)
            {
                int n = droppedObjects.Count;
                for (int i = 0; i < n; i++)
                {
                    if (IsUnChecked(droppedObjects[i]))
                    {
                        return;
                    }
                }

                // removing from suspicious if there is IF before drop
                suspiciousObjectDrops.Remove(node);
            }
        }
    }
}
