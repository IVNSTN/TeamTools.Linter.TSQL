using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0724", "REDUNDANT_INDEX_OPTION")]
    [IndexRule]
    internal sealed class RedundantIndexOptionRule : AbstractRule
    {
        private readonly IDictionary<IndexOptionKind, KeyValuePair<string, bool>> defaultOptions = new Dictionary<IndexOptionKind, KeyValuePair<string, bool>>();

        public RedundantIndexOptionRule() : base()
        {
            AddDefaultOption(IndexOptionKind.PadIndex, "PAD_INDEX", false);
            AddDefaultOption(IndexOptionKind.SortInTempDB, "SORT_IN_TEMPDB", false);
            AddDefaultOption(IndexOptionKind.IgnoreDupKey, "IGNORE_DUP_KEY", false);
            AddDefaultOption(IndexOptionKind.StatisticsNoRecompute, "STATISTICS_NORECOMPUTE", false);
            AddDefaultOption(IndexOptionKind.StatisticsIncremental, "STATISTICS_INCREMENTAL", false);
            AddDefaultOption(IndexOptionKind.DropExisting, "DROP_EXISTING ", false);
            AddDefaultOption(IndexOptionKind.Online, "ONLINE", false);
            AddDefaultOption(IndexOptionKind.AllowRowLocks, "ALLOW_ROW_LOCKS", true);
            AddDefaultOption(IndexOptionKind.AllowPageLocks, "ALLOW_PAGE_LOCKS", true);
            AddDefaultOption(IndexOptionKind.OptimizeForSequentialKey, "OPTIMIZE_FOR_SEQUENTIAL_KEY", false);
        }

        public override void Visit(UniqueConstraintDefinition node) => ValidateIndexOptions(node.IndexOptions);

        public override void Visit(IndexStatement node) => ValidateIndexOptions(node.IndexOptions);

        public override void Visit(IndexDefinition node) => ValidateIndexOptions(node.IndexOptions);

        private void ValidateIndexOptions(IList<IndexOption> options)
        {
            var redundantOptions = options
                .OfType<IndexStateOption>()
                .Where(opt => defaultOptions.ContainsKey(opt.OptionKind)
                    && defaultOptions[opt.OptionKind].Value == (opt.OptionState == OptionState.On))
                .ToDictionary(opt => defaultOptions[opt.OptionKind].Key, opt => opt);

            if (!redundantOptions.Any())
            {
                return;
            }

            HandleNodeError(redundantOptions.FirstOrDefault().Value, string.Join(", ", redundantOptions.Select(o => $"{o.Key} {o.Value.OptionState.ToString().ToUpperInvariant()}")));
        }

        private void AddDefaultOption(IndexOptionKind opt, string name, bool isOn)
            => defaultOptions.Add(opt, new KeyValuePair<string, bool>(name, isOn));
    }
}
