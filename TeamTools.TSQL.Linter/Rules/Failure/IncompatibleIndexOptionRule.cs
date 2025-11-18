using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0742", "INDEX_OPTIONS_INCOMPATIBLE")]
    [IndexRule]
    internal sealed class IncompatibleIndexOptionRule : AbstractRule
    {
        public IncompatibleIndexOptionRule() : base()
        {
        }

        public override void Visit(CreateColumnStoreIndexStatement node) => ValidateIndexOptions(false, true, node.FilterPredicate != null, node.IndexOptions, ViolationHandlerWithMessage);

        public override void Visit(CreateIndexStatement node) => ValidateIndexOptions(node.Unique, false, node.FilterPredicate != null, node.IndexOptions, ViolationHandlerWithMessage);

        public override void Visit(IndexDefinition node) => ValidateIndexOptions(node.Unique, false, node.FilterPredicate != null, node.IndexOptions, ViolationHandlerWithMessage);

        private static void ValidateIndexOptions(bool isUnique, bool isColumnStore, bool isFiltered, IList<IndexOption> indexOptions, Action<TSqlFragment, string> callback)
        {
            if (indexOptions.Count == 0)
            {
                return;
            }

            // TODO : stop collecting, start reporting
            var incompatibleOptions = new Dictionary<IndexOptionKind, string>();

            if ((!isUnique || isFiltered) && IsOptionOn(indexOptions, IndexOptionKind.IgnoreDupKey))
            {
                incompatibleOptions.Add(IndexOptionKind.IgnoreDupKey, "IGNORE_DUP_KEY can be specified on unique and not filtered index only");
            }

            if (isFiltered)
            {
                incompatibleOptions.Add(IndexOptionKind.StatisticsIncremental, "STATISTICS_INCREMENTAL cannot be specified on filtered index");
            }

            if (isColumnStore)
            {
                incompatibleOptions.Add(IndexOptionKind.Online, "Columnstore index cannot be ONLINE");
            }

            if (IsOptionOn(indexOptions, IndexOptionKind.PadIndex)
            && !indexOptions.HasOption(IndexOptionKind.FillFactor))
            {
                incompatibleOptions.Add(IndexOptionKind.PadIndex, "PAD_INDEX is useful only when FILLFACTOR specified");
            }

            if (IsOptionOn(indexOptions, IndexOptionKind.Resumable) && !IsOptionOn(indexOptions, IndexOptionKind.Online))
            {
                incompatibleOptions.Add(IndexOptionKind.Resumable, "RESUMABLE needs ONLINE");
            }

            if (indexOptions.HasOption(IndexOptionKind.MaxDuration)
            && !IsOptionOn(indexOptions, IndexOptionKind.Resumable))
            {
                incompatibleOptions.Add(IndexOptionKind.MaxDuration, "MAXDURATION needs RESUMABLE");
            }

            if (incompatibleOptions.Count == 0)
            {
                return;
            }

            var badOptions = indexOptions
                .Where(opt => incompatibleOptions.ContainsKey(opt.OptionKind))
                .ToDictionary(opt => opt, opt => incompatibleOptions[opt.OptionKind]);

            if (badOptions.Count == 0)
            {
                return;
            }

            var badOption = badOptions.First();
            callback(badOption.Key, badOption.Value);
        }

        private static bool IsOptionOn(IEnumerable<IndexOption> indexOptions, IndexOptionKind optionKind)
            => indexOptions.Any(opt => opt.OptionKind == optionKind && opt is IndexStateOption o && o.OptionState == OptionState.On);
    }
}
