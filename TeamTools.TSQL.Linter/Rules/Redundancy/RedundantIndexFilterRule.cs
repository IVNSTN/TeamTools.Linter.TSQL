using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Support IN / NOT IN predicates
    [RuleIdentity("RD0849", "REDUNDANT_INDEX_FILTER")]
    [IndexRule]
    internal sealed class RedundantIndexFilterRule : ScriptAnalysisServiceConsumingRule
    {
        public RedundantIndexFilterRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript script)
        {
            var info = GetService<TableDefinitionElementsEnumerator>(script);

            if (info.Tables.Count == 0)
            {
                return;
            }

            foreach (var tbl in info.Tables)
            {
                // Extracting defined column-related constraints in <column, constraint definition> format
                var columnConstraints = ExtractColumnConstraints(tbl.Value);
                if (columnConstraints.Count == 0)
                {
                    continue;
                }

                foreach (var idx in info.Indices(tbl.Key).OfType<SqlIndexInfo>())
                {
                    // Extracting index filter definition in <column, predicate> format
                    if (idx.Definition is CreateIndexStatement index && index.FilterPredicate != null)
                    {
                        // CREATE INDEX statement
                        ValidateIndexPredicate(ExtractColumnFilters(index.FilterPredicate), columnConstraints);
                    }
                    else if (idx.Definition is IndexDefinition ix && ix.FilterPredicate != null)
                    {
                        // Inline index definition
                        ValidateIndexPredicate(ExtractColumnFilters(ix.FilterPredicate), columnConstraints);
                    }
                }
            }
        }

        private static IDictionary<string, IList<BooleanExpressionParts>> ExtractColumnConstraints(SqlTableDefinition tbl)
        {
            var constraints = new Dictionary<string, IList<BooleanExpressionParts>>(StringComparer.OrdinalIgnoreCase);

            // NOT NULL constraints
            foreach (var col in tbl.Columns)
            {
                // ContainsKey for protection agains broken table definition with col name dups
                if (!col.Value.IsNullable && !constraints.ContainsKey(col.Key))
                {
                    var colConstraints = new List<BooleanExpressionParts>();
                    colConstraints.Add(MakeNotNullConstraint(col.Value.Node.ColumnIdentifier));
                    constraints.Add(col.Key, colConstraints);
                }
            }

            // CHECK constraints
            for (int i = tbl.Node.TableConstraints.Count - 1; i >= 0; i--)
            {
                var cstr = tbl.Node.TableConstraints[i];
                if (cstr is CheckConstraintDefinition chk)
                {
                    // Extract simple expressions, take those who reference a column
                    foreach (var expr in ExtractNestedExpressions(chk.CheckCondition))
                    {
                        var colFilter = expr;
                        if (!(colFilter.FirstExpression is ColumnReferenceExpression))
                        {
                            // let the column reference be on the left side
                            colFilter = colFilter.Mirror();
                        }

                        if (colFilter.FirstExpression is ColumnReferenceExpression colRef)
                        {
                            var colName = colRef.MultiPartIdentifier.GetLastIdentifier().Value;
                            if (!constraints.TryGetValue(colName, out var colConstraints))
                            {
                                colConstraints = new List<BooleanExpressionParts>();
                                constraints.Add(colName, colConstraints);
                            }

                            colConstraints.Add(colFilter);
                        }
                    }
                }
            }

            return constraints;
        }

        private static IEnumerable<KeyValuePair<string, BooleanExpressionParts>> ExtractColumnFilters(BooleanExpression indexPredicate)
        {
            foreach (var expr in ExtractNestedExpressions(indexPredicate))
            {
                var colFilter = expr;
                if (!(colFilter.FirstExpression is ColumnReferenceExpression))
                {
                    // let the column reference be on the left side
                    colFilter = colFilter.Mirror();
                }

                if (colFilter.FirstExpression is ColumnReferenceExpression colRef)
                {
                    string colName = colRef.MultiPartIdentifier.Identifiers.GetFullName();
                    yield return new KeyValuePair<string, BooleanExpressionParts>(colName, colFilter);
                }
            }
        }

        private static IEnumerable<BooleanExpressionParts> ExtractNestedExpressions(BooleanExpression node)
        {
            node = BooleanExpressionPartsExtractor.ExtractExpression(node);

            // OR's are harder to understand and filtered index syntax does not currently support OR
            if (node is BooleanBinaryExpression bin && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                foreach (var e in ExtractNestedExpressions(bin.FirstExpression))
                {
                    yield return e;
                }

                foreach (var e in ExtractNestedExpressions(bin.SecondExpression))
                {
                    yield return e;
                }
            }

            yield return BooleanExpressionNormalizer.Normalize(node);
        }

        private static BooleanExpressionParts MakeNotNullConstraint(Identifier column)
        {
            var colReference = new ColumnReferenceExpression
            {
                MultiPartIdentifier = new MultiPartIdentifier(),
                // Things below are required for GetFragmentCleanedText()
                FirstTokenIndex = column.FirstTokenIndex,
                LastTokenIndex = column.LastTokenIndex,
                ScriptTokenStream = column.ScriptTokenStream,
            };
            colReference.MultiPartIdentifier.Identifiers.Add(column);

            return new BooleanExpressionParts
            {
                FirstExpression = colReference,
                ComparisonType = BooleanComparisonConverter.ToEqualityComparison(false),
            };
        }

        // Matching column constraint predicates with index filter predicates via column name
        private void ValidateIndexPredicate(IEnumerable<KeyValuePair<string, BooleanExpressionParts>> columnPredicates, IDictionary<string, IList<BooleanExpressionParts>> columnConstraints)
        {
            foreach (var columnPredicate in columnPredicates)
            {
                string columnName = columnPredicate.Key;

                if (columnConstraints.TryGetValue(columnName, out var constraints))
                {
                    for (int i = constraints.Count - 1; i >= 0; i--)
                    {
                        if (columnPredicate.Value.Equals(constraints[i]))
                        {
                            HandleNodeError(columnPredicate.Value.OriginalExpression, columnName);
                        }
                    }
                }
            }
        }
    }
}
