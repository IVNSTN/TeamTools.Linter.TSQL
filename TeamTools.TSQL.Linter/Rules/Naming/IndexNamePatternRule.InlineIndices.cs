using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Inline index processing.
    /// </summary>
    internal partial class IndexNamePatternRule
    {
        private void ValidateInlineInidices(TSqlFragment node, string tableSchema, string tableName)
        {
            var inlineVisitor = new InlineIndexVisitor(nd => ValidateInlineIndexDefinition(nd, tableSchema, tableName));
            node.AcceptChildren(inlineVisitor);
        }

        private void ValidateInlineIndexDefinition(IndexDefinition node, string tableSchema, string tableName)
        {
            string expectedName = IndexNameBuilder.Build(
                tableSchema,
                tableName,
                false,
                false,
                node.Unique,
                node.FilterPredicate != null,
                node.IncludeColumns?.Count > 0,
                node.Columns.ExtractNames());

            ValidateIndexName(node.Name, expectedName);
        }

        private sealed class InlineIndexVisitor : TSqlFragmentVisitor
        {
            private readonly Action<IndexDefinition> callback;

            public InlineIndexVisitor(Action<IndexDefinition> callback)
            {
                this.callback = callback;
            }

            public override void Visit(IndexDefinition node) => callback(node);
        }
    }
}
