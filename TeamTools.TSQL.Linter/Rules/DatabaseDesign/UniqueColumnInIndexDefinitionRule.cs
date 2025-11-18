using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0909", "INDEX_DUP_COLUMN")]
    [IndexRule]
    internal sealed class UniqueColumnInIndexDefinitionRule : AbstractRule
    {
        public UniqueColumnInIndexDefinitionRule() : base()
        {
        }

        public override void Visit(UniqueConstraintDefinition node)
        {
            var columns = node.Columns
                .Select(col => col.Column)
                .ToArray();

            ValidateColumnNames(columns, ViolationHandlerWithMessage);
        }

        public override void Visit(CreateIndexStatement node)
        {
            var columns = node.Columns
                .Select(col => col.Column)
                .Union(node.IncludeColumns)
                .ToArray();

            ValidateColumnNames(columns, ViolationHandlerWithMessage);
        }

        public override void Visit(IndexDefinition node)
        {
            var columns = node.Columns
                .Select(col => col.Column)
                .Union(node.IncludeColumns)
                .ToArray();

            ValidateColumnNames(columns, ViolationHandlerWithMessage);
        }

        private static void ValidateColumnNames(ColumnReferenceExpression[] cols, Action<TSqlFragment, string> callback)
        {
            var foundNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in cols)
            {
                string colName = col.MultiPartIdentifier.GetLastIdentifier().Value;

                if (!foundNames.Add(colName))
                {
                    callback(col, colName);
                }
            }
        }
    }
}
