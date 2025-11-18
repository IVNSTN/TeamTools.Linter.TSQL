using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0166", "INSERT_COLUMN_ALIAS_MATCH_TARGET")]
    internal sealed class InsertedColumnAliasMatchTargetRule : AbstractRule
    {
        private const int MaxReportedViolationsPerStatement = 5;

        public InsertedColumnAliasMatchTargetRule() : base()
        {
        }

        public override void Visit(InsertStatement node)
        {
            if (node.InsertSpecification.Columns?.Count == 0)
            {
                // insert columns not specified
                return;
            }

            if (!(node.InsertSpecification.InsertSource is SelectInsertSource selins))
            {
                // insert-exec or insert-values
                return;
            }

            var spec = selins.Select.GetQuerySpecification();
            if (spec is null)
            {
                return;
            }

            string nameMismatches = ValidateColumnAliases(node.InsertSpecification.Columns, spec.SelectElements, out TSqlFragment firstMismatch);

            if (string.IsNullOrEmpty(nameMismatches))
            {
                return;
            }

            HandleNodeError(firstMismatch ?? spec, nameMismatches);
        }

        // Note, it does not catch OutputClause without INTO
        public override void Visit(OutputIntoClause node)
        {
            string nameMismatches = ValidateColumnAliases(node.IntoTableColumns, node.SelectColumns, out TSqlFragment firstMismatch);

            if (string.IsNullOrEmpty(nameMismatches))
            {
                return;
            }

            HandleNodeError(firstMismatch ?? node, nameMismatches);
        }

        private static string ValidateColumnAliases(IList<ColumnReferenceExpression> targetCols, IList<SelectElement> srcCols, out TSqlFragment firstNode)
        {
            int n = targetCols.Count > srcCols.Count ? srcCols.Count : targetCols.Count;
            string srcName;
            string dstName;
            StringBuilder nameMismatches = null;
            int mismatchesCount = 0;
            firstNode = default;

            for (int i = 0; i < n; i++)
            {
                srcName = "n/a";
                if (srcCols[i] is SelectScalarExpression scalarExpr)
                {
                    if (null != scalarExpr.ColumnName)
                    {
                        srcName = scalarExpr.ColumnName.Value;
                    }
                    else if (scalarExpr.Expression is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier?.Count > 0)
                    {
                        srcName = colRef.MultiPartIdentifier.GetLastIdentifier().Value;
                    }
                }

                dstName = targetCols[i].MultiPartIdentifier.GetLastIdentifier().Value;

                if (!string.Equals(srcName, dstName, StringComparison.InvariantCulture))
                {
                    if (firstNode is null)
                    {
                        firstNode = srcCols[i];
                        nameMismatches = ObjectPools.StringBuilderPool.Get();
                    }
                    else
                    {
                        nameMismatches.Append("; ");
                    }

                    mismatchesCount++;
                    nameMismatches.Append($"{srcName}!={dstName}");

                    if (mismatchesCount >= MaxReportedViolationsPerStatement)
                    {
                        break;
                    }
                }
            }

            if (nameMismatches != null)
            {
                var result = nameMismatches.ToString();
                ObjectPools.StringBuilderPool.Return(nameMismatches);
                return result;
            }

            return default;
        }
    }
}
