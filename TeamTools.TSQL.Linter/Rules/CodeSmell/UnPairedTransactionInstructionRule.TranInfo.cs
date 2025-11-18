using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// TranInfo.
    /// </summary>
    internal partial class UnPairedTransactionInstructionRule
    {
        private enum TranStatementKind
        {
            BeginTran,
            SaveTran,
            CommitTran,
            RollbackTran,
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
    }
}
