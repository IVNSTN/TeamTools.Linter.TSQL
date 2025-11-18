using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0724", "REDUNDANT_INDEX_OPTION")]
    [IndexRule]
    internal sealed class RedundantIndexOptionRule : AbstractRule
    {
        private static readonly Dictionary<IndexOptionKind, Tuple<string, bool>> DefaultOptions = new Dictionary<IndexOptionKind, Tuple<string, bool>>();

        static RedundantIndexOptionRule()
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

        public RedundantIndexOptionRule() : base()
        {
        }

        public override void Visit(UniqueConstraintDefinition node) => ValidateIndexOptions(node.IndexOptions);

        public override void Visit(IndexStatement node) => ValidateIndexOptions(node.IndexOptions);

        public override void Visit(IndexDefinition node) => ValidateIndexOptions(node.IndexOptions);

        private static void AddDefaultOption(IndexOptionKind opt, string name, bool isOn)
            => DefaultOptions.Add(opt, new Tuple<string, bool>(name, isOn));

        private void ValidateIndexOptions(IList<IndexOption> options)
        {
            const string stateOn = " ON";
            const string stateOff = " OFF";

            IndexStateOption firstBadItem = default;
            StringBuilder msg = default;

            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i] is IndexStateOption opt
                && DefaultOptions.TryGetValue(opt.OptionKind, out var defaultState)
                && defaultState.Item2 == (opt.OptionState == OptionState.On))
                {
                    if (firstBadItem is null)
                    {
                        firstBadItem = opt;
                        msg = ObjectPools.StringBuilderPool.Get();
                    }
                    else
                    {
                        msg.Append(", ");
                    }

                    msg
                        .Append(defaultState.Item1)
                        .Append(opt.OptionState == OptionState.On ? stateOn : stateOff);
                }
            }

            if (msg != null)
            {
                HandleNodeError(firstBadItem, msg.ToString());
                ObjectPools.StringBuilderPool.Return(msg);
            }
        }
    }
}
