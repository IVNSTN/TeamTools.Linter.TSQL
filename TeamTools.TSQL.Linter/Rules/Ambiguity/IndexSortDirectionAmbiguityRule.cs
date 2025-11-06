using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0155", "INDEX_DIRECTION_AMBIGUOUS")]
    [IndexRule]
    internal sealed class IndexSortDirectionAmbiguityRule : AbstractRule
    {
        public IndexSortDirectionAmbiguityRule() : base()
        {
        }

        public override void Visit(CreateIndexStatement node)
        {
            if (!ValidateIndexColumnDefinition(node.Columns))
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(IndexDefinition node)
        {
            if (!ValidateIndexColumnDefinition(node.Columns))
            {
                HandleNodeError(node);
            }
        }

        private static bool ValidateIndexColumnDefinition(IList<ColumnWithSortOrder> cols)
        {
            bool allDirectionsSpecified = true;
            bool descExists = false;

            foreach (var col in cols)
            {
                allDirectionsSpecified &= col.SortOrder != SortOrder.NotSpecified;
                descExists |= col.SortOrder == SortOrder.Descending;
            }

            return allDirectionsSpecified || !descExists;
        }
    }
}
