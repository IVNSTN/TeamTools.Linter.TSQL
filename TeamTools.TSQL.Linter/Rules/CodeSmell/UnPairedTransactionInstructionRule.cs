using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0920", "UNPAIRED_TRAN_STATEMENT")]

    internal sealed class UnPairedTransactionInstructionRule : AbstractRule
    {
        public UnPairedTransactionInstructionRule() : base()
        {
        }

        private enum TranStatementKind
        {
            BeginTran,
            SaveTran,
            CommitTran,
            RollbackTran,
        }

        public override void Visit(TSqlScript node)
        {
            var callVisitor = new CallVisitor();
            node.AcceptChildren(callVisitor);

            if (!callVisitor.Trans.Any())
            {
                return;
            }

            AnalyzeTranStatements(callVisitor.Trans);
        }

        private void AnalyzeTranStatements(IList<TranInfo> transactions)
        {
            var openedTrans = new Dictionary<string, TranStatementKind>(StringComparer.OrdinalIgnoreCase);

            foreach (var call in transactions)
            {
                switch (call.Stmt)
                {
                    case TranStatementKind.BeginTran:
                    case TranStatementKind.SaveTran:
                        {
                            if (openedTrans.ContainsKey(call.TranName))
                            {
                                if (openedTrans[call.TranName] == TranStatementKind.BeginTran
                                || openedTrans[call.TranName] == TranStatementKind.SaveTran)
                                {
                                    HandleNodeError(call.Node, call.TranName);
                                }
                                else
                                {
                                    openedTrans[call.TranName] = call.Stmt;
                                }
                            }
                            else
                            {
                                openedTrans.Add(call.TranName, call.Stmt);
                            }

                            break;
                        }

                    // TODO : analyze paths and catch simultanuous calls possible in the same code flow path
                    case TranStatementKind.CommitTran:
                    case TranStatementKind.RollbackTran:
                        {
                            if (openedTrans.ContainsKey(call.TranName))
                            {
                                // TODO : for now treating "multiple commits"/rollback after commit
                                // as OK because some of them may be in CATCH block. Waiting for more
                                // comprehensive rule implementation
                                if (openedTrans[call.TranName] == TranStatementKind.BeginTran
                                || openedTrans[call.TranName] == TranStatementKind.SaveTran
                                || openedTrans[call.TranName] == TranStatementKind.CommitTran)
                                {
                                    openedTrans[call.TranName] = call.Stmt;
                                }
                                else
                                {
                                    HandleNodeError(call.Node, call.TranName);
                                }
                            }
                            else
                            {
                                openedTrans.Add(call.TranName, call.Stmt);

                                HandleNodeError(call.Node, call.TranName);
                            }

                            break;
                        }
                }
            }

            ValidateFinalTranStates(transactions, openedTrans);
        }

        private void ValidateFinalTranStates(IList<TranInfo> transactions, IDictionary<string, TranStatementKind> openedTrans)
        {
            var closedStates = new List<TranStatementKind>
            {
                TranStatementKind.RollbackTran,
                TranStatementKind.CommitTran,
            };

            // TODO : respect xact_abort option
            var looseTrans = openedTrans
                .Where(t => !closedStates.Contains(t.Value))
                .Join(
                    transactions,
                    openTran => openTran.Key,
                    tranInfo => tranInfo.TranName,
                    (openTran, tranInfo) => new KeyValuePair<string, TSqlFragment>(openTran.Key, tranInfo.Node),
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var looseTran in looseTrans)
            {
                HandleNodeError(looseTran.Value, looseTran.Key);
            }
        }

        private class TranInfo
        {
            public TranInfo(TSqlFragment node, string tranName, TranStatementKind tranStatementKind, bool isXactAbortOn)
            {
                this.Node = node;
                this.TranName = tranName;
                this.Stmt = tranStatementKind;
                this.IsXactAbortOn = isXactAbortOn;
            }

            public TSqlFragment Node { get; }

            public string TranName { get; }

            public TranStatementKind Stmt { get; }

            public bool IsXactAbortOn { get; }
        }

        private class CallVisitor : TSqlFragmentVisitor
        {
            // FIXME : It must be "true" for a trigger body
            private bool isXactAbortOn = false;

            public IList<TranInfo> Trans { get; } = new List<TranInfo>();

            public override void Visit(TransactionStatement node)
            {
                string tranName = node.Name?.Value ?? "<noname>";

                Trans.Add(new TranInfo(node, tranName, GetStatementKind(node), isXactAbortOn));
            }

            public override void Visit(PredicateSetStatement node)
            {
                if (node.Options.HasFlag(SetOptions.XactAbort))
                {
                    isXactAbortOn = node.IsOn;
                }
            }

            private static TranStatementKind GetStatementKind(TransactionStatement node)
            {
                if (node is BeginTransactionStatement)
                {
                    return TranStatementKind.BeginTran;
                }
                else if (node is SaveTransactionStatement)
                {
                    return TranStatementKind.SaveTran;
                }
                else if (node is CommitTransactionStatement)
                {
                    return TranStatementKind.CommitTran;
                }

                return TranStatementKind.RollbackTran;
            }
        }
    }
}
