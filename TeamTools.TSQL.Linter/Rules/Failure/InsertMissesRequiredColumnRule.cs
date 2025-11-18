using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0980", "INSERT_MISSES_REQUIRED_COL")]
    internal sealed class InsertMissesRequiredColumnRule : AbstractRule
    {
        public InsertMissesRequiredColumnRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var validator = new RequiredInsertColumnsValidator(ViolationHandlerWithMessage);
            node.AcceptChildren(validator);
        }

        internal sealed class RequiredInsertColumnsValidator : InsertColumnsValidator
        {
            public RequiredInsertColumnsValidator(Action<TSqlFragment, string> callback) : base(callback)
            {
            }

            protected override void ValidateInsertedColumns(TSqlFragment node, string tableName, IList<ColumnReferenceExpression> cols)
            {
                if (cols is null || cols.Count == 0)
                {
                    // all cols are inserted
                    // possible mismatching column number in insert source is controlled by a separate rule
                    return;
                }

                if (!TableColumns.TryGetValue(tableName, out var tblCols))
                {
                    // no data to compare to
                    return;
                }

                bool identityIsOn = IsIdentityInsertOnFor(tableName);

                var requiredColNames = tblCols
                    .Where(col => col.Value == ColType.RequiredCol || (identityIsOn && col.Value == ColType.IdentityCol))
                    .Select(col => col.Key);

                if (!requiredColNames.Any())
                {
                    // all cols are optional
                    return;
                }

                var targetColNames = new HashSet<string>(cols.ExtractNames(), StringComparer.OrdinalIgnoreCase);

                string missingCols = string.Join(
                    ", ",
                    requiredColNames
                        .Where(colName => !targetColNames.Contains(colName))
                        .OrderBy(_ => _));

                if (string.IsNullOrEmpty(missingCols))
                {
                    // no required cols are missing in inserted columns
                    return;
                }

                Callback(node, $"table = {tableName}, cols = {missingCols}");
            }
        }
    }
}
