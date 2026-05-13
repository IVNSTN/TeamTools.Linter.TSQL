using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class IsolationLevelContradictionRule
    {
        private sealed class IsolationLevelSwitchVisitor : TSqlFragmentVisitor
        {
            private static readonly Dictionary<TableHintKind, int> HintWeights = new Dictionary<TableHintKind, int>
            {
                { TableHintKind.NoLock, 1 },
                { TableHintKind.ReadUncommitted, 1 },
                { TableHintKind.ReadCommitted, 2 },
                { TableHintKind.RepeatableRead, 3 },
                { TableHintKind.HoldLock, 4 },
                { TableHintKind.Serializable, 4 },
                { TableHintKind.TabLockX, 3 },
                { TableHintKind.UpdLock, 3 },
                { TableHintKind.ReadPast, 2 },
            };

            private static readonly Dictionary<IsolationLevel, int> IsolationWeights = new Dictionary<IsolationLevel, int>
            {
                { IsolationLevel.ReadUncommitted, 1 },
                { IsolationLevel.ReadCommitted, 2 },
                { IsolationLevel.RepeatableRead, 3 },
                { IsolationLevel.Serializable, 4 },
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

            private static readonly Dictionary<IsolationLevel, string> IsolationNames = new Dictionary<IsolationLevel, string>
            {
                { IsolationLevel.ReadUncommitted, "READ UNCOMMITTED" },
                { IsolationLevel.ReadCommitted, "READ COMMITTED" },
                { IsolationLevel.RepeatableRead, "REPEATABLE READ" },
                { IsolationLevel.Serializable, "SERIALIZABLE" },
            };

            public IsolationLevelSwitchVisitor(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment, string> Callback { get; }

            private IsolationLevel DefinedIsolationLevel { get; set; } = IsolationLevel.None;

            public override void Visit(SetTransactionIsolationLevelStatement node)
            {
                DefinedIsolationLevel = node.Level;
            }

            public override void Visit(TableHint node)
            {
                OverrideIsolationLevel(node);
            }

            private void OverrideIsolationLevel(TableHint hint)
            {
                if (DefinedIsolationLevel == IsolationLevel.None)
                {
                    // no directive was found yet
                    return;
                }

                if (!HintWeights.TryGetValue(hint.HintKind, out int overridenIsolationLevel)
                || !IsolationWeights.TryGetValue(DefinedIsolationLevel, out int definedIsolationWeight))
                {
                    // something unsupported
                    return;
                }

                if (Math.Abs(overridenIsolationLevel - definedIsolationWeight) > 1)
                {
                    // too big difference
                    Callback(hint, $"{HintNames[hint.HintKind]} vs {IsolationNames[DefinedIsolationLevel]}");
                }
            }
        }
    }
}
