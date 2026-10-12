using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class MultipleModificationAtomicRule
    {
        private sealed class DmlOutOfTranDetector : BaseDmlDetector
        {
            private static readonly string Msg = Strings.ViolationDetails_MultipleModificationAtomicRule_NoTransaction;
            private int tranCount = 0;

            public DmlOutOfTranDetector(Action<TSqlFragment, string> callback) : base(callback, Msg)
            { }

            private int TranCount
            {
                get
                {
                    return tranCount;
                }

                set
                {
                    if (value < 0)
                    {
                        // in case of consequtive conditional COMMIT/ROLLBACK statements
                        tranCount = 0;
                    }
                    else
                    {
                        tranCount = value;
                    }
                }
            }

            public override void Visit(BeginTransactionStatement node) => TranCount++;

            public override void Visit(CommitTransactionStatement node) => TranCount--;

            public override void Visit(SaveTransactionStatement node)
            {
                // save tran does not increate @@TRANCOUNT but means we are in transaction, maybe external one
                if (TranCount == 0)
                {
                    TranCount++;
                }
            }

            public override void Visit(RollbackTransactionStatement node)
            {
                if (node.Name is null)
                {
                    // rollback not to a checkpoint rollbacks whole tran
                    // rollback to checkpoint does not affect TRANCOUNT
                    TranCount = 0;
                }
            }

            protected override void WalkAllIfElseBranches(IfStatement node)
            {
                // saving before walking all if-else branches
                int dmlCountBeforeGoingNested = DmlCount;
                int tranCountBeforeGoingNested = TranCount;
                int maxDmlCountAfterNested = 0;
                int maxTranCountAfterNested = 0;

                var nestedIf = node.ThenStatement;
                var nestedElse = node.ElseStatement;

                while (nestedIf != null)
                {
                    // resetting back to values that were before IF for each independent branch
                    DmlCount = dmlCountBeforeGoingNested;
                    TranCount = tranCountBeforeGoingNested;

                    // Going deeper
                    nestedIf.Accept(this);

                    if (DmlCount > maxDmlCountAfterNested)
                    {
                        maxDmlCountAfterNested = DmlCount;
                    }

                    if (TranCount > maxTranCountAfterNested)
                    {
                        maxTranCountAfterNested = TranCount;
                    }

                    if (nestedElse is IfStatement elseIf)
                    {
                        nestedIf = elseIf.ThenStatement;
                        nestedElse = elseIf.ElseStatement;
                    }
                    else
                    {
                        nestedIf = nestedElse;
                        nestedElse = null;
                    }
                }

                // resetting to max values after all possible branches analysis
                DmlCount = maxDmlCountAfterNested;
                TranCount = maxTranCountAfterNested;
            }

            protected override bool CanDmlBeIgnored(DataModificationStatement node)
            {
                if (TranCount > 0)
                {
                    // only DMLs outside of tran should be counted
                    return true;
                }

                return base.CanDmlBeIgnored(node);
            }
        }
    }
}
