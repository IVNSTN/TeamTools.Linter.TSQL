using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class IsolationLevelChaosPerQueryRule
    {
        private sealed class IsolationLevelHintVisitor : TSqlFragmentVisitor
        {
            private static readonly Dictionary<TableHintKind, int> HintWeights = new Dictionary<TableHintKind, int>
            {
                { TableHintKind.NoLock, 1 },
                { TableHintKind.ReadUncommitted, 1 },
                { TableHintKind.ReadCommitted, 2 },
                { TableHintKind.RepeatableRead, 3 },
                { TableHintKind.HoldLock, 4 },
                { TableHintKind.Serializable, 4 },
                { TableHintKind.TabLockX, 4 },
                { TableHintKind.UpdLock, 3 },
                { TableHintKind.ReadPast, 2 },
            };

            private static readonly Dictionary<TableHintKind, string> HintNames = new Dictionary<TableHintKind, string>
            {
                { TableHintKind.NoLock, "NOLOCK" },
                { TableHintKind.ReadUncommitted, "READUNCOMMITTED" },
                { TableHintKind.ReadCommitted, "READCOMMITTED" },
                { TableHintKind.RepeatableRead, "REPEATABLEREAD" },
                { TableHintKind.HoldLock, "HOLDLOCK" },
                { TableHintKind.Serializable, "SERIALIZABLE" },
                { TableHintKind.TabLockX, "TABLOCKX" },
                { TableHintKind.UpdLock, "UPDLOCK" },
                { TableHintKind.ReadPast, "READPAST" },
            };

            private TableHintKind priorIsolationLevelMin = TableHintKind.None;
            private int priorIsolationLevelMinWeight = 0;

            private TableHintKind priorIsolationLevelMax = TableHintKind.None;
            private int priorIsolationLevelMaxWeight = 0;

            public IsolationLevelHintVisitor(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment, string> Callback { get; }

            public override void Visit(TableHint node)
            {
                SetNextIsolationLevel(node);
            }

            private void SetNextIsolationLevel(TableHint node)
            {
                var nextIsolationLevel = node.HintKind;

                if (!HintWeights.TryGetValue(nextIsolationLevel, out int nextIsolationLevelWeight))
                {
                    // something unsupported
                    return;
                }

                if (priorIsolationLevelMinWeight == 0)
                {
                    // the very first hint in a query
                    priorIsolationLevelMin = nextIsolationLevel;
                    priorIsolationLevelMinWeight = nextIsolationLevelWeight;
                    priorIsolationLevelMax = nextIsolationLevel;
                    priorIsolationLevelMaxWeight = nextIsolationLevelWeight;

                    return;
                }

                if (Math.Abs(nextIsolationLevelWeight - priorIsolationLevelMinWeight) > 1)
                {
                    // too big difference
                    Callback(node, $"{HintNames[nextIsolationLevel]} vs {HintNames[priorIsolationLevelMin]}");
                }
                else if (Math.Abs(nextIsolationLevelWeight - priorIsolationLevelMaxWeight) > 1)
                {
                    // too big difference
                    Callback(node, $"{HintNames[nextIsolationLevel]} vs {HintNames[priorIsolationLevelMax]}");
                }

                // FIXME : actually this allows moderate downscaling from SERIALIZABLE
                // to NOCLOCK in a single query without getting violation.
                if (nextIsolationLevelWeight > priorIsolationLevelMaxWeight)
                {
                    priorIsolationLevelMax = nextIsolationLevel;
                    priorIsolationLevelMaxWeight = nextIsolationLevelWeight;
                }
                else if (nextIsolationLevelWeight < priorIsolationLevelMinWeight)
                {
                    priorIsolationLevelMin = nextIsolationLevel;
                    priorIsolationLevelMinWeight = nextIsolationLevelWeight;
                }
            }
        }
    }
}
