using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// TranVisitor.
    /// </summary>
    internal partial class UnPairedTransactionInstructionRule
    {
        private class TranVisitor : TSqlFragmentVisitor
        {
            // TODO : It must be "true" for a trigger body
            private bool isXactAbortOn = false;

            public List<TranInfo> Trans { get; } = new List<TranInfo>();

            public override void Visit(TransactionStatement node)
            {
                string tranName = node.Name?.Value ?? NoNameTranId;

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

                if (node is SaveTransactionStatement)
                {
                    return TranStatementKind.SaveTran;
                }

                if (node is CommitTransactionStatement)
                {
                    return TranStatementKind.CommitTran;
                }

                return TranStatementKind.RollbackTran;
            }
        }
    }
}
