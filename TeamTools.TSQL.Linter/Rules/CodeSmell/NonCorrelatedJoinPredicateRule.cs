using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0852", "NON_CORRELATED_JOIN_PREDICATE")]
    internal sealed class NonCorrelatedJoinPredicateRule : AbstractRule
    {
        public NonCorrelatedJoinPredicateRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if ((node.FromClause?.TableReferences?.Count ?? 0) == 0)
            {
                // no FROM
                return;
            }

            if (node.FromClause.TableReferences.Count == 1
            && !(node.FromClause.TableReferences[0] is JoinTableReference))
            {
                // simple select with single source
                return;
            }

            ValidateJoins(node.FromClause.TableReferences);
        }

        // It extracts names of a source possible for use as addressing: alias if provided,
        // fully qualified name, name with "dbo" schema omitted
        private static ICollection<string> ExtractSourceNames(TableReference source, bool takePrior = false)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (source is QualifiedJoin qj)
            {
                source = takePrior ? qj.SecondTableReference : qj.FirstTableReference;
            }

            if (source is TableReferenceWithAlias aliased && aliased.Alias != null)
            {
                names.Add(aliased.Alias.Value);
            }

            if (source is NamedTableReference named)
            {
                names.Add(named.SchemaObject.GetFullName());

                if (named.SchemaObject.SchemaIdentifier is null
                || string.Equals(named.SchemaObject.SchemaIdentifier.Value, TSqlDomainAttributes.DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    // default schema can be omitted in references
                    names.Add(named.SchemaObject.BaseIdentifier.Value);
                }
            }

            return names;
        }

        // At least one condition in a predicate must refer to then given table source and some other table source
        private static bool IsValidJoin(QualifiedJoin join)
        {
            var leftNames = ExtractSourceNames(join.FirstTableReference, true);
            var rightNames = ExtractSourceNames(join.SecondTableReference);

            return IsJoinPredicateCorrelated(join.SearchCondition, leftNames, rightNames);
        }

        // Both parts of a boolean comparison expression must be linked to at least 2 sources
        private static bool IsJoinPredicateCorrelated(BooleanExpression predicate, ICollection<string> leftNames, ICollection<string> rightNames)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            if (predicate is BooleanBinaryExpression bin)
            {
                if (bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                {
                    // for AND either of expression must be fine
                    return IsJoinPredicateCorrelated(bin.FirstExpression, leftNames, rightNames)
                        || IsJoinPredicateCorrelated(bin.SecondExpression, leftNames, rightNames);
                }
                else
                {
                    // for OR all of the expressions must be fine
                    return IsJoinPredicateCorrelated(bin.FirstExpression, leftNames, rightNames)
                        && IsJoinPredicateCorrelated(bin.SecondExpression, leftNames, rightNames);
                }
            }

            if (predicate is BooleanComparisonExpression cmp)
            {
                return CorrelatedColumnReferenceDetector.HasCorrelatedRefs(cmp.FirstExpression, cmp.SecondExpression, leftNames, rightNames);
            }

            if (predicate is LikePredicate like)
            {
                return CorrelatedColumnReferenceDetector.HasCorrelatedRefs(like.FirstExpression, like.SecondExpression, leftNames, rightNames);
            }

            if (predicate is BooleanTernaryExpression between)
            {
                return CorrelatedColumnReferenceDetector.HasCorrelatedRefs(between.FirstExpression, between.SecondExpression, leftNames, rightNames)
                    && CorrelatedColumnReferenceDetector.HasCorrelatedRefs(between.FirstExpression, between.ThirdExpression, leftNames, rightNames);
            }

            // IS [NOT] NULL and such cannot be correlated
            return false;
        }

        // Recursivly expanding nested/linked joins to find all joins and join predicates
        private static IEnumerable<QualifiedJoin> ExpandJoins(JoinTableReference join)
        {
            if (join is QualifiedJoin qj)
            {
                yield return qj;
            }

            // Expanding nested/linked joins recursively
            if (join.FirstTableReference is JoinTableReference firstSrc)
            {
                foreach (var j in ExpandJoins(firstSrc))
                {
                    yield return j;
                }
            }

            if (join.SecondTableReference is JoinTableReference secondSrc)
            {
                foreach (var j in ExpandJoins(secondSrc))
                {
                    yield return j;
                }
            }
        }

        private void ValidateJoins(IList<TableReference> sources)
        {
            for (int i = sources.Count - 1; i >= 0; i--)
            {
                var src = sources[i];
                if (src is JoinTableReference jtr)
                {
                    foreach (var join in ExpandJoins(jtr))
                    {
                        if (!IsValidJoin(join))
                        {
                            HandleNodeError(join.SearchCondition);
                        }
                    }
                }
            }
        }

        // It finds all the column references and tries to realize if it is linked to either
        // of two joined sources. If both parts of a given boolean comparison expression (join predicate)
        // are linked to something and at least one of the parts is linked particularly to left or right source
        // used in the JOIN then everything is fine.
        private sealed class CorrelatedColumnReferenceDetector : TSqlFragmentVisitor
        {
            private readonly ICollection<string> leftNames;
            private readonly ICollection<string> rightNames;

            public CorrelatedColumnReferenceDetector(ICollection<string> leftNames, ICollection<string> rightNames)
            {
                this.leftNames = leftNames;
                this.rightNames = rightNames;
            }

            [Flags]
            private enum ColumnSourceFlags
            {
                None = 0,
                Left = 1,
                Right = 2,
                Another = 4,
                Both = Left | Right,
                External = Right | Another,
            }

            private bool ColumnsDontHaveTableAlias { get; set; }

            private ColumnSourceFlags CurrentDetections { get; set; } = ColumnSourceFlags.None;

            private ColumnSourceFlags LeftDetections { get; set; } = ColumnSourceFlags.None;

            private ColumnSourceFlags RightDetections { get; set; } = ColumnSourceFlags.None;

            public static bool HasCorrelatedRefs(ScalarExpression first, ScalarExpression second, ICollection<string> leftNames, ICollection<string> rightNames)
            {
                var instance = new CorrelatedColumnReferenceDetector(leftNames, rightNames);

                instance.LeftDetections = instance.DetectSourceRefs(first);
                instance.RightDetections = instance.DetectSourceRefs(second);

                // Both "left" and "right" sources must be mentioned on one or two sides of the predicate.
                // Also some other source from the query may be mentioned with either "left" or "right" source.
                // If there are columns referenced without explicitly defined source alias then
                // violation should not be reported because everything may be fine in fact.
                // Another rule should report such unqualified references as violations.
                return (instance.LeftDetections | instance.RightDetections) == ColumnSourceFlags.Both
                    || (instance.LeftDetections | instance.RightDetections) > ColumnSourceFlags.Another
                    || instance.ColumnsDontHaveTableAlias;
            }

            // Catching all column references
            public override void Visit(ColumnReferenceExpression node)
            {
                var ids = node.MultiPartIdentifier.Identifiers;
                if (ids.Count < 2)
                {
                    ColumnsDontHaveTableAlias = true;
                    return;
                }

                DetectCurrentSourceRef(BuildTableReference(ids));
            }

            // It builds full source name written before column ref
            private static string BuildTableReference(IList<Identifier> nameParts)
            {
                if (nameParts.Count == 2)
                {
                    // <tbl>.<col> => <tbl>
                    return nameParts[0].Value;
                }

                var sb = ObjectPools.StringBuilderPool.Get();

                // All name parts except the last one (column name) are needed
                for (int i = 0, n = nameParts.Count - 1; i < n; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(TSqlDomainAttributes.NamePartSeparator);
                    }

                    sb.Append(nameParts[i].Value);
                }

                var fullName = sb.ToString();
                ObjectPools.StringBuilderPool.Return(sb);
                return fullName;
            }

            // It compares given column source name to known names extracted
            // from 2 sources defined in a JOIN.
            private void DetectCurrentSourceRef(string sourceRef)
            {
                if (leftNames.Contains(sourceRef))
                {
                    CurrentDetections |= ColumnSourceFlags.Left;
                }
                else if (rightNames.Contains(sourceRef))
                {
                    CurrentDetections |= ColumnSourceFlags.Right;
                }
                else
                {
                    // TODO : Validate that alias/table name exists in the query? Or let another rule do this.
                    CurrentDetections |= ColumnSourceFlags.Another;
                }
            }

            private ColumnSourceFlags DetectSourceRefs(TSqlFragment node)
            {
                // Result intermediate result holder and run the visitor
                CurrentDetections = ColumnSourceFlags.None;
                node.Accept(this);
                return CurrentDetections;
            }
        }
    }
}
