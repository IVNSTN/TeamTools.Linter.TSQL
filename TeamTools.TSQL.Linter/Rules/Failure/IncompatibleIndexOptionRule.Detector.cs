using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class IncompatibleIndexOptionRule
    {
        private sealed class IncompatibleOptionDetector
        {
            private static readonly Dictionary<IndexOptionKind, string> IncompatibilityWarning = new Dictionary<IndexOptionKind, string>
            {
                { IndexOptionKind.IgnoreDupKey, Strings.ViolationDetails_IncompatibleIndexOptionRule_IgnoreDupKeyUnique },
                { IndexOptionKind.StatisticsIncremental, Strings.ViolationDetails_IncompatibleIndexOptionRule_StatIncFilter },
                { IndexOptionKind.PadIndex, Strings.ViolationDetails_IncompatibleIndexOptionRule_PadIndexFillFactor },
                { IndexOptionKind.Online, Strings.ViolationDetails_IncompatibleIndexOptionRule_ColumnStoreOnline },
                { IndexOptionKind.Resumable, Strings.ViolationDetails_IncompatibleIndexOptionRule_ResumableOnline },
                { IndexOptionKind.MaxDuration, Strings.ViolationDetails_IncompatibleIndexOptionRule_MaxDurationResumable },
            };

            private readonly Action<TSqlFragment, string> callback;
            private readonly Dictionary<IndexOptionKind, IndexOption> enabledOptions;

            public IncompatibleOptionDetector(IList<IndexOption> indexOptions, Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
                enabledOptions = DetectEnabledOptions(indexOptions);
            }

            public void ValidateIndexOptions(bool isUnique, bool isColumnStore, bool isFiltered)
            {
                if (enabledOptions.Count == 0)
                {
                    return;
                }

                if (!isUnique || isFiltered)
                {
                    // IGNORE_DUP_KEY can be set on unique and not filtered index only
                    DetectIncompatibility(IndexOptionKind.IgnoreDupKey);
                }

                if (isFiltered)
                {
                    // filtered index cannot have incremental stats
                    DetectIncompatibility(IndexOptionKind.StatisticsIncremental);
                }

                if (!enabledOptions.ContainsKey(IndexOptionKind.FillFactor))
                {
                    // PAD_INDEX makes sense with FILLFACTOR only
                    DetectIncompatibility(IndexOptionKind.PadIndex);
                }

                if (isColumnStore)
                {
                    // COLUMNSTORE index cannot be ONLINE
                    DetectIncompatibility(IndexOptionKind.Online);
                }

                if (!enabledOptions.ContainsKey(IndexOptionKind.Online))
                {
                    // RESUMABLE needs ONLINE
                    DetectIncompatibility(IndexOptionKind.Resumable);
                }

                if (!enabledOptions.ContainsKey(IndexOptionKind.Resumable))
                {
                    // MAX_DURATION needs RESUMABLE
                    DetectIncompatibility(IndexOptionKind.MaxDuration);
                }
            }

            private static Dictionary<IndexOptionKind, IndexOption> DetectEnabledOptions(IList<IndexOption> indexOptions)
            {
                var optState = new Dictionary<IndexOptionKind, IndexOption>();

                for (int i = indexOptions.Count - 1; i >= 0; i--)
                {
                    var opt = indexOptions[i];
                    if (!(opt is IndexStateOption o) || o.OptionState == OptionState.On)
                    {
                        optState.Add(opt.OptionKind, opt);
                    }
                }

                return optState;
            }

            private void DetectIncompatibility(IndexOptionKind incompatibleOption)
            {
                if (enabledOptions.TryGetValue(incompatibleOption, out var badOpt))
                {
                    callback(badOpt, IncompatibilityWarning[incompatibleOption]);
                }
            }
        }
    }
}
