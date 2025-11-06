using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0130", "ROWVERSION_UNIQUE")]
    [IndexRule]
    internal sealed class RowVersionIndexUniqueRule : AbstractRule
    {
        public RowVersionIndexUniqueRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var tableVisitor = new TableIndicesVisitor();
            node.AcceptChildren(tableVisitor);

            if (tableVisitor.Table == null || tableVisitor.Indices.Count == 0)
            {
                return;
            }

            var columnVisitor = new ColumnVisitor(new string[] { "ROWVERSION", "TIMESTAMP" });
            tableVisitor.Table.AcceptChildren(columnVisitor);

            if (columnVisitor.Columns.Count == 0)
            {
                return;
            }

            CheckIndices(columnVisitor.Columns, tableVisitor.Indices);
        }

        private void CheckIndices(IList<string> columns, IList<TableIndexInfo> indices)
        {
            foreach (TableIndexInfo idx in indices)
            {
                if (idx.Unique)
                {
                    continue;
                }

                if (idx.Columns.Any(c =>
                    columns.Any(
                        i => i.Equals(
                            c.Column.MultiPartIdentifier.Identifiers[c.Column.MultiPartIdentifier.Count - 1].Value,
                            StringComparison.OrdinalIgnoreCase))))
                {
                    HandleNodeError(idx.Definition);
                }
            }
        }
    }
}
