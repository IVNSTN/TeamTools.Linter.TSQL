using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
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

        public override void Visit(OutputIntoClause node)
        {
            string nameMismatches = ValidateColumnAliases(node.IntoTableColumns, node.SelectColumns, out TSqlFragment firstMismatch);

            if (string.IsNullOrEmpty(nameMismatches))
            {
                return;
            }

            HandleNodeError(firstMismatch ?? node, nameMismatches);
        }

        private string ValidateColumnAliases(IList<ColumnReferenceExpression> targetCols, IList<SelectElement> srcCols, out TSqlFragment firstNode)
        {
            int n = targetCols.Count > srcCols.Count ? srcCols.Count : targetCols.Count;
            string srcName;
            string dstName;
            List<string> nameMismatches = new List<string>();
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
                        var ident = colRef.MultiPartIdentifier.Identifiers;
                        srcName = ident[ident.Count - 1].Value;
                    }
                }

                dstName = targetCols[i].MultiPartIdentifier.Identifiers[targetCols[i].MultiPartIdentifier.Identifiers.Count - 1].Value;

                if (!string.Equals(srcName, dstName, StringComparison.InvariantCulture))
                {
                    nameMismatches.Add(string.Format("{0}!={1}", srcName, dstName));
                    if (firstNode == null)
                    {
                        firstNode = srcCols[i];
                    }
                }
            }

            return string.Join(
                "; ",
                nameMismatches.Count > MaxReportedViolationsPerStatement ? nameMismatches.GetRange(0, MaxReportedViolationsPerStatement) : nameMismatches);
        }
    }
}
