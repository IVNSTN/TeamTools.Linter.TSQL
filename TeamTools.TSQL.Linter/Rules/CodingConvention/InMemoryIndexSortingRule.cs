using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0768", "INMEMORY_INDEX_SORTING")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [IndexRule]
    [InMemoryRule]
    internal sealed class InMemoryIndexSortingRule : AbstractRule
    {
        public InMemoryIndexSortingRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == TableOptionKind.MemoryOptimized))
            {
                return;
            }

            node.Definition.Indexes
                .Where(idx => idx.IndexType.IndexTypeKind == IndexTypeKind.NonClustered)
                .SelectMany(idx => idx.Columns)
                .Where(col => col.SortOrder == SortOrder.NotSpecified)
                .ToList()
                .ForEach(HandleNodeError);
        }
    }
}
