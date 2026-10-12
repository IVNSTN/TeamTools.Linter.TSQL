using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class XactStateCheckMissingRule
    {
        private sealed class AfterTranVisitor : MultipleModificationAtomicRule.BaseDmlDetector
        {
            private static readonly string XactStateFuncName = "XACT_STATE";
            private static readonly string TranCountGlobalVarName = "@@TRANCOUNT";

            private int tranCount = 1; // imitating possible external transaction
            private bool afterError;

            public AfterTranVisitor(Action<TSqlFragment, string> callback) : base(callback, default)
            { }

            private bool XactStateVerified { get; set; }

            private bool TranCountVerified { get; set; }

            private bool BatchInterrupted { get; set; }

            private bool XactAbortEnabled { get; set; }

            private bool AfterError
            {
                get
                {
                    return afterError;
                }

                set
                {
                    if (value)
                    {
                        // new possible error - new XACT_STATE verification needed
                        XactStateVerified = false;
                    }

                    afterError = value;
                }
            }

            private int TranCount
            {
                get
                {
                    return tranCount;
                }

                set
                {
                    // recheck needed after each transaction manipulation
                    TranCountVerified = false;

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

            // In an atomic block and trigger XACT_ABORT is ON by default
            public override void Visit(BeginEndAtomicBlockStatement node) => XactAbortEnabled = true;

            public override void Visit(TriggerStatementBody node) => XactAbortEnabled = true;

            public override void Visit(PredicateSetStatement node)
            {
                if (node.Options.HasFlag(SetOptions.XactAbort))
                {
                    XactAbortEnabled = node.IsOn;
                }
            }

            public override void ExplicitVisit(RaiseErrorStatement node)
            {
                // RAISERROR does not break batch execution but can make transaction uncommittable
                // if severity is > 10 and XACT_ABORT is enabled
                if (node.SecondParameter is IntegerLiteral l && int.TryParse(l.Value, out var severity)
                && severity > TSqlDomainAttributes.MaxInfoRaisErrorSeverity)
                {
                    AfterError = XactAbortEnabled;
                }
            }

            public override void ExplicitVisit(ThrowStatement node)
            {
                AfterError = true;
                // THROW breaks batch execution
                BatchInterrupted = true;
            }

            public override void ExplicitVisit(ReturnStatement node)
            {
                // RETURN breaks batch execution
                BatchInterrupted = true;
            }

            public override void ExplicitVisit(TryCatchStatement node)
            {
                AfterError = true;

                // statements inside CATCH execute on error only
                // and everything after CATCH can be executed after error too
                // thus not visiting TRY block and setting AfterError true for everything beneath
                node.CatchStatements?.AcceptChildren(this);
            }

            // if transaction is uncommittable then it must be rolled back first
            // before opening new transaction or doing something with the current one
            public override void Visit(BeginTransactionStatement node)
            {
                Validate(node);
                TranCount++;
            }

            public override void Visit(SaveTransactionStatement node)
            {
                Validate(node);
                // save tran does not increate @@TRANCOUNT but means we are in transaction, maybe external one
                if (TranCount == 0)
                {
                    TranCount++;
                }
            }

            // for Commit one must check XACT_STATE first
            public override void Visit(CommitTransactionStatement node)
            {
                Validate(node);
                TranCount--;
            }

            public override void Visit(RollbackTransactionStatement node)
            {
                ValidateRollback(node);

                if (node.Name is null)
                {
                    // rollback not to a checkpoint rollbacks whole tran
                    // rollback to checkpoint does not affect TRANCOUNT
                    TranCount = 0;
                }
            }

            // Looking for @@TRANCOUNT references
            public override void Visit(GlobalVariableExpression node)
            {
                if (!TranCountVerified
                && node.Name.Equals(TranCountGlobalVarName, StringComparison.OrdinalIgnoreCase))
                {
                    TranCountVerified = true;
                }
            }

            // Looking for XACT_STATE calls
            public override void Visit(FunctionCall node)
            {
                if (!XactStateVerified
                && node.FunctionName.Value.Equals(XactStateFuncName, StringComparison.OrdinalIgnoreCase))
                {
                    XactStateVerified = true;
                }
            }

            // Before running DML the XACT_STATE must be verified
            public override void Visit(DataModificationStatement node)
            {
                if (CanDmlBeIgnored(node))
                {
                    return;
                }

                Validate(node);
            }

            protected override bool CanDmlBeIgnored(DataModificationStatement node)
            {
                // Table variables do not participate in user-defined transactions
                return MultipleModificationAtomicRule.RealTablesDetector.IsNoRealTableDml(node, false);
            }

            protected override void WalkAllIfElseBranches(IfStatement node)
            {
                // saving before walking all if-else branches
                bool afterErrorBeforeGoingNested = AfterError;
                bool xactStateVerifiedBeforeGoingNested = XactStateVerified;
                bool batchInterruptedBeforeGoingNested = BatchInterrupted;
                int tranCountBeforeGoingNested = TranCount;
                bool maxAfterErrorAfterNested = false;
                bool maxXactStateVerifiedAfterNested = false;
                bool maxBatchInterruptedAfterNested = false;
                int maxTranCountAfterNested = 0;

                var nestedIf = node.ThenStatement;
                var nestedElse = node.ElseStatement;
                var predicate = node.Predicate;

                while (nestedIf != null)
                {
                    // resetting back to values that were before IF for each independent branch
                    AfterError = afterErrorBeforeGoingNested;
                    XactStateVerified = xactStateVerifiedBeforeGoingNested;
                    BatchInterrupted = batchInterruptedBeforeGoingNested;
                    TranCount = tranCountBeforeGoingNested;

                    // Predicate might contain XACT_STATE or @@TRANCOUNT check
                    predicate?.Accept(this);
                    // Going deeper
                    nestedIf.Accept(this);

                    if (AfterError)
                    {
                        maxAfterErrorAfterNested = true;
                    }

                    if (XactStateVerified)
                    {
                        maxXactStateVerifiedAfterNested = true;
                    }

                    if (BatchInterrupted)
                    {
                        maxBatchInterruptedAfterNested = true;
                    }

                    if (TranCount > maxTranCountAfterNested || TranCount == 0)
                    {
                        // nested tran opened or overall ROLLBACK/COMMIT called
                        maxTranCountAfterNested = TranCount;
                    }

                    if (nestedElse is IfStatement elseIf)
                    {
                        nestedIf = elseIf.ThenStatement;
                        nestedElse = elseIf.ElseStatement;
                        predicate = elseIf.Predicate;
                    }
                    else
                    {
                        nestedIf = nestedElse;
                        nestedElse = null;
                        predicate = null;
                    }
                }

                if (!afterErrorBeforeGoingNested && maxAfterErrorAfterNested
                && !batchInterruptedBeforeGoingNested && maxBatchInterruptedAfterNested)
                {
                    // THROW + RETURN - "case closed"
                    maxAfterErrorAfterNested = false;
                    maxBatchInterruptedAfterNested = false;
                }

                // resetting to max values after all possible branches analysis
                AfterError = maxAfterErrorAfterNested;
                XactStateVerified = maxXactStateVerifiedAfterNested;
                TranCount = maxTranCountAfterNested;
                BatchInterrupted = maxBatchInterruptedAfterNested;
            }

            private void ValidateRollback(TSqlFragment node)
            {
                if (TranCountVerified)
                {
                    // @@TRANCOUNT checked
                    return;
                }

                Validate(node);
            }

            private void Validate(TSqlFragment node)
            {
                if (BatchInterrupted)
                {
                    // THROW and RETURN break batch, nothing afterwards is executed
                    return;
                }

                if (XactStateVerified)
                {
                    // XACT_STATE checked
                    return;
                }

                if (AfterError && TranCount > 0)
                {
                    Callback(node, default);
                    // reporting once per bad situation
                    Reset();
                }
            }

            private void Reset()
            {
                XactStateVerified = false;
                TranCountVerified = false;
                AfterError = false;
            }
        }
    }
}
