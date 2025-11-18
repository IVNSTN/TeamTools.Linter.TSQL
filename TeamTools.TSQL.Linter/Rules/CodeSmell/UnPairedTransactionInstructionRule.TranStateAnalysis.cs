using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Tran state analysis.
    /// </summary>
    internal partial class UnPairedTransactionInstructionRule
    {
        private void AnalyzeTranStatements(List<TranInfo> transactions)
        {
            var openedTrans = new Dictionary<string, TranStatementKind>(StringComparer.OrdinalIgnoreCase);

            int n = transactions.Count;
            for (int i = 0; i < n; i++)
            {
                var call = transactions[i];

                switch (call.Stmt)
                {
                    case TranStatementKind.BeginTran:
                    case TranStatementKind.SaveTran:
                        {
                            if (!openedTrans.TryAdd(call.TranName, call.Stmt))
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

                            break;
                        }

                    // TODO : analyze paths and catch simultanuous calls possible in the same code flow path
                    case TranStatementKind.CommitTran:
                    case TranStatementKind.RollbackTran:
                        {
                            if (openedTrans.TryGetValue(call.TranName, out var tranState))
                            {
                                // TODO : for now treating "multiple commits"/rollback after commit
                                // as OK because some of them may be in CATCH block. Waiting for more
                                // comprehensive rule implementation
                                if (tranState == TranStatementKind.BeginTran
                                || tranState == TranStatementKind.SaveTran
                                || tranState == TranStatementKind.CommitTran)
                                {
                                    openedTrans[call.TranName] = call.Stmt;
                                }
                                else
                                {
                                    // tran was not opened to be closed
                                    ReportError(call.Node, call.TranName);
                                }
                            }
                            else
                            {
                                // registering unknown tran state
                                openedTrans.Add(call.TranName, call.Stmt);

                                // but reporting an error because this tran was not started
                                ReportError(call.Node, call.TranName);
                            }

                            break;
                        }
                }
            }

            ValidateFinalTranStates(transactions, openedTrans);
        }

        private void ValidateFinalTranStates(List<TranInfo> transactions, IDictionary<string, TranStatementKind> openedTrans)
        {
            // TODO : respect xact_abort option
            // tran is not in final state
            var looseTrans = openedTrans
                .Where(t => t.Value != TranStatementKind.RollbackTran && t.Value != TranStatementKind.CommitTran)
                .Join(
                    transactions,
                    openTran => openTran.Key,
                    tranInfo => tranInfo.TranName,
                    (openTran, tranInfo) => new KeyValuePair<string, TSqlFragment>(openTran.Key, tranInfo.Node),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var looseTran in looseTrans)
            {
                ReportError(looseTran.Value, looseTran.Key);
            }
        }
    }
}
