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

        public override void Visit(TSqlBatch node)
        {
            var dropVisitor = new DropVisitor();
            node.AcceptChildren(dropVisitor);

            if (!dropVisitor.SuspiciousDrops.Any())
            {
                return;
            }

            var preceidingConditionVisitor = new ConditionalDropVisitor(dropVisitor.SuspiciousDrops);
            node.AcceptChildren(preceidingConditionVisitor);

            foreach (var drop in dropVisitor.SuspiciousDrops)
            {
                HandleNodeError(drop);
            }
        }

        private class DropVisitor : TSqlFragmentVisitor
        {
            public List<TSqlFragment> SuspiciousDrops { get; } = new List<TSqlFragment>();

            public override void Visit(DropObjectsStatement node)
            {
                if (!node.IsIfExists)
                {
                    SuspiciousDrops.Add(node);
                }
            }

            public override void Visit(DropTypeStatement node)
            {
                if (!node.IsIfExists)
                {
                    SuspiciousDrops.Add(node);
                }
            }
        }

        private class ConditionalDropVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> suspiciousDrops;

            public ConditionalDropVisitor(List<TSqlFragment> suspiciousDrops)
            {
                this.suspiciousDrops = suspiciousDrops;
            }

            public override void Visit(IfStatement node)
            {
                var existanceChecks = ExtractObjectExistanceChecks(node.Predicate, false);

                if (!existanceChecks.Any())
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

        private class NestedDropsVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> suspiciousObjectDrops;
            private readonly IEnumerable<string> objectExistanceChecks;

            public NestedDropsVisitor(List<TSqlFragment> suspiciousObjectDrops, IEnumerable<string> objectExistanceChecks)
            {
                this.suspiciousObjectDrops = suspiciousObjectDrops;
                this.objectExistanceChecks = objectExistanceChecks;
            }

            public override void Visit(DropObjectsStatement node) => MarkNonSuspiciousIfChecked(node, node.Objects);

            public override void Visit(DropTypeStatement node) => MarkNonSuspiciousIfChecked(node, new List<SchemaObjectName> { node.Name });

            private void MarkNonSuspiciousIfChecked(TSqlFragment node, IList<SchemaObjectName> droppedObjects)
            {
                var missingChecks = droppedObjects.Where(obj => !objectExistanceChecks.Contains(obj.GetFullName(), StringComparer.OrdinalIgnoreCase));

                if (missingChecks.Any())
                {
                    return;
                }

                // removing from suspicious if there is IF before drop
                suspiciousObjectDrops.Remove(node);
            }
        }
    }
}
