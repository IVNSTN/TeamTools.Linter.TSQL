using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0979", "INSERT_MODIFIES_READONLY_COL")]
    internal sealed class InsertModifiesReadonlyColumnRule : AbstractRule
    {
        public InsertModifiesReadonlyColumnRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var validator = new ReadonlyInsertColumnsValidator(HandleNodeError);
            node.AcceptChildren(validator);
        }

        internal sealed class ReadonlyInsertColumnsValidator : InsertColumnsValidator
        {
            public ReadonlyInsertColumnsValidator(Action<TSqlFragment, string> callback) : base(callback)
            {
            }

            protected override void ValidateInsertedColumns(TSqlFragment node, string tableName, IList<ColumnReferenceExpression> cols)
            {
                if (cols is null)
                {
                    // all cols are inserted
                    // possible mismatching column number in insert source is controlled by a separate rule
                    return;
                }

                if (!TableColumns.ContainsKey(tableName))
                {
                    // no data to compare to
                    return;
                }

                bool identityIsOn = IsIdentityInsertOnFor(tableName);

                var readonlyColNames = TableColumns[tableName]
                    .Where(col => col.Value == ColType.ComputedCol || (!identityIsOn && col.Value == ColType.IdentityCol))
                    .Select(col => col.Key);

                if (!readonlyColNames.Any())
                {
                    // all cols are editable
                    return;
                }

                var targetColNames = cols
                    .Select(col => col.MultiPartIdentifier.Identifiers.Last().Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(colName => colName, _ => true, StringComparer.OrdinalIgnoreCase);

                string missingCols = string.Join(
                    ", ",
                    readonlyColNames
                        .Where(colName => targetColNames.ContainsKey(colName))
                        .OrderBy(_ => _));

                if (missingCols == "")
                {
                    // no readonly cols mentioned in inserted columns
                    return;
                }

                Callback(node, "table = " + tableName + ", cols = " + missingCols);
            }
        }
    }
}
