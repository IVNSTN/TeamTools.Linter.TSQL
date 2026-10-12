using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class RedundantOptionCleanupRule
    {
        private sealed class OptionCleanupVisitor : TSqlFragmentVisitor
        {
            private static readonly HashSet<GeneralSetCommandType> BatchScopedGeneralCommands = new HashSet<GeneralSetCommandType>
            {
                GeneralSetCommandType.DateFirst,
                GeneralSetCommandType.DateFormat,
                GeneralSetCommandType.Language,
                GeneralSetCommandType.DeadlockPriority,
                GeneralSetCommandType.LockTimeout,
            };

            private static readonly HashSet<SetOptions> BatchScopedPredicateOptions = new HashSet<SetOptions>
            {
                SetOptions.NoCount,
                SetOptions.XactAbort,
                SetOptions.AnsiNulls,
                SetOptions.QuotedIdentifier,
                SetOptions.AnsiNullDfltOff,
                SetOptions.AnsiNullDfltOn,
                SetOptions.AnsiPadding,
                SetOptions.AnsiWarnings,
                SetOptions.ArithAbort,
                SetOptions.ConcatNullYieldsNull,
                SetOptions.NumericRoundAbort,
                SetOptions.ImplicitTransactions,
                SetOptions.CursorCloseOnCommit,
            };

            // If false then no statements that could be affected by SET has been met before
            // thus next SET is the first real command in a module and should not be reported.
            private bool statementSeen = false;

            public TSqlFragment ViolationNode { get; private set; }

            // These could be affected by option switch
            public override void Visit(DataModificationStatement node) => Reset();

            public override void Visit(ExecuteStatement node) => Reset();

            public override void Visit(QuerySpecification node) => Reset();

            public override void Visit(SetVariableStatement node) => Reset();

            // To check if there was something before option switch
            public override void Visit(TSqlStatement node)
            {
                if (node is PredicateSetStatement
                || node is SetTransactionIsolationLevelStatement
                || node is BeginEndBlockStatement
                || node is TryCatchStatement)
                {
                    // Ignoring SET commands.
                    // GeneralSetCommand is derived from TSqlFragment, not TSqlStatement.
                    return;
                }

                statementSeen = true;
            }

            // Option change
            public override void Visit(PredicateSetStatement node)
            {
                if (!statementSeen)
                {
                    return;
                }

                foreach (SetOptions opt in BatchScopedPredicateOptions)
                {
                    if (node.Options.HasFlag(opt))
                    {
                        ViolationNode = node;
                        return;
                    }
                }
            }

            public override void Visit(GeneralSetCommand node)
            {
                if (!statementSeen)
                {
                    return;
                }

                if (BatchScopedGeneralCommands.Contains(node.CommandType))
                {
                    ViolationNode = node;
                }
            }

            public override void Visit(SetTransactionIsolationLevelStatement node)
            {
                if (!statementSeen)
                {
                    return;
                }

                ViolationNode = node;
            }

            private void Reset() => ViolationNode = null;
        }
    }
}
