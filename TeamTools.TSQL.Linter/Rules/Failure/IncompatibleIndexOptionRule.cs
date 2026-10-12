using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0742", "INDEX_OPTIONS_INCOMPATIBLE")]
    [IndexRule]
    internal sealed partial class IncompatibleIndexOptionRule : AbstractRule
    {
        public IncompatibleIndexOptionRule() : base()
        {
        }

        public override void Visit(CreateColumnStoreIndexStatement node) => ValidateIndexOptions(false, true, node.FilterPredicate != null, node.IndexOptions);

        public override void Visit(CreateIndexStatement node) => ValidateIndexOptions(node.Unique, false, node.FilterPredicate != null, node.IndexOptions);

        public override void Visit(IndexDefinition node) => ValidateIndexOptions(node.Unique, false, node.FilterPredicate != null, node.IndexOptions);

        private void ValidateIndexOptions(bool isUnique, bool isColumnStore, bool isFiltered, IList<IndexOption> indexOptions)
        {
            if (indexOptions.Count == 0)
            {
                return;
            }

            new IncompatibleOptionDetector(indexOptions, ViolationHandlerWithMessage).ValidateIndexOptions(isUnique, isColumnStore, isFiltered);
        }
    }
}
