using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0851", "FAKE_OUTER_JOIN")]
    internal sealed partial class FakeOuterJoinRule : AbstractRule
    {
        public FakeOuterJoinRule() : base()
        {
        }

        // TODO : FROM a LEFT JOIN b ON ... INNER JOIN c ON c.id = b.id
        public override void Visit(QuerySpecification node)
        {
            if (node.WhereClause?.SearchCondition is null)
            {
                // no WHERE or it is a WHERE CURRENT OF <cursor>
                return;
            }

            if ((node.FromClause?.TableReferences?.Count ?? 0) == 0)
            {
                // no FROM
                return;
            }

            var outerSources = ExtractOuterSources(node.FromClause.TableReferences);

            if (outerSources.Count == 0)
            {
                // no outer joins
                return;
            }

            DetectPredicatesReferencingOuterSources(node.WhereClause.SearchCondition, outerSources);
        }

        private void DetectOuterSourceReference(ScalarExpression possibleColReference, ICollection<string> outerSources)
        {
            if (!(ExpandExpression(possibleColReference) is ColumnReferenceExpression colRef))
            {
                return;
            }

            var referencedIdentifiers = colRef.MultiPartIdentifier.Identifiers;

            if (referencedIdentifiers.Count <= 1)
            {
                // Single name means column name with no link to a specific source.
                // Not possible to make outer source matching.
                return;
            }

            string referencedSourceName = GetReferencedSourceFullName(referencedIdentifiers);

            if (outerSources.Contains(referencedSourceName))
            {
                HandleNodeError(possibleColReference, referencedSourceName);
            }
            else if (referencedIdentifiers.Count == 3 && string.Equals(referencedIdentifiers[0].Value, TSqlDomainAttributes.DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                referencedSourceName = referencedIdentifiers[1].Value;

                // Reference 'dbo.tbl.col' is the same as 'tbl.col' thus checking without schema as well
                if (outerSources.Contains(referencedSourceName))
                {
                    HandleNodeError(possibleColReference, referencedSourceName);
                }
            }
        }

        private void DetectPredicatesReferencingOuterSources(BooleanExpression wherePredicate, ICollection<string> outerSources)
        {
            foreach (BooleanExpression predicate in ExpandPredicate(wherePredicate))
            {
                if (predicate is BooleanComparisonExpression cmp)
                {
                    DetectOuterSourceReference(cmp.FirstExpression, outerSources);
                    DetectOuterSourceReference(cmp.SecondExpression, outerSources);
                }
                else if (predicate is BooleanIsNullExpression isnull && isnull.IsNot)
                {
                    // col IS NOT NULL
                    DetectOuterSourceReference(isnull.Expression, outerSources);
                }
                else if (predicate is InPredicate inpred)
                {
                    DetectOuterSourceReference(inpred.Expression, outerSources);
                }
                else if (predicate is LikePredicate likepred)
                {
                    DetectOuterSourceReference(likepred.FirstExpression, outerSources);
                }
            }
        }
    }
}
