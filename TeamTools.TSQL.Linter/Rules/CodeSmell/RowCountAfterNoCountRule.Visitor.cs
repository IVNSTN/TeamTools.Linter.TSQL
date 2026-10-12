using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // Docs: https://learn.microsoft.com/en-us/sql/t-sql/functions/rowcount-transact-sql
    internal partial class RowCountAfterNoCountRule
    {
        private sealed class RowCountChangerVisitor : VisitorWithCallback
        {
            private static readonly string RowCountVariable = "@@ROWCOUNT";

            public RowCountChangerVisitor(bool rowCountInitialized, Action<TSqlFragment> callback) : base(callback)
            {
                RowCountValueMakesSense = rowCountInitialized;
            }

            public bool RowCountValueMakesSense { get; private set; }

            public override void Visit(GlobalVariableExpression node)
            {
                if (RowCountValueMakesSense)
                {
                    return;
                }

                // accessing @@ROWCOUNT after it was reset
                if (node.Name.Equals(RowCountVariable, StringComparison.OrdinalIgnoreCase))
                {
                    Callback(node);
                }
            }

            // These statements fill ROWCOUNT with a meaningful value.
            // This visit covers SELECT and DML statements.
            public override void Visit(StatementWithCtesAndXmlNamespaces node) => SetRowCount(node);

            public override void Visit(ExecuteStatement node) => SetRowCount(node);

            public override void Visit(ReceiveStatement node) => SetRowCount(node);

            // These statements reset ROWCOUNT to 0 or 1.
            // ExplicitVisit is used to prevent double-visiting of nested nodes
            // after RowCountValueMakesSense was reset to false
            public override void ExplicitVisit(PredicateSetStatement node) => ResetRowCount(node);

            // Abstract classes cannot be visited via ExplicitVisit but they
            // do not support global variable references so this is fine
            public override void Visit(TransactionStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(DeclareCursorStatement node) => ResetRowCount(node);

            public override void Visit(CursorStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(DeclareVariableStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(UseStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(PrintStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(ReadTextStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(SetVariableStatement node) => ResetRowCount(node);

            public override void ExplicitVisit(TryCatchStatement node) => ResetRowCount(node);

            // All if-else paths must be validated separately
            public override void ExplicitVisit(IfStatement node) => WalkAllIfElseBranches(node);

            private void ResetRowCount(TSqlFragment node)
            {
                // For example, if @@ROWCOUNT is used in SET @var = @@ROWCOUNT
                // and it has meaningful value then this usage is fine
                // RowCountValueMakesSense must be updated after the statement only
                node.AcceptChildren(this);
                RowCountValueMakesSense = false;
            }

            private void SetRowCount(TSqlFragment node)
            {
                // For example, if @@ROWCOUNT is used inside SELECT
                // then this usage must be validated first
                // RowCountValueMakesSense must be updated after the statement only
                node.AcceptChildren(this);
                RowCountValueMakesSense = true;
            }

            private void WalkAllIfElseBranches(IfStatement node)
            {
                bool maxRowCountValueMakesSense = RowCountValueMakesSense;
                bool rowCountValueMakesSenseBeforeGoingNested = RowCountValueMakesSense;

                var nestedIf = node.ThenStatement;
                var nestedElse = node.ElseStatement;

                // e.g. IF @@ROWCOUNT > 0 - predicate must be validated
                node.Predicate.Accept(this);

                while (nestedIf != null)
                {
                    RowCountValueMakesSense = rowCountValueMakesSenseBeforeGoingNested;

                    // Going deeper
                    nestedIf.Accept(this);

                    if (RowCountValueMakesSense)
                    {
                        maxRowCountValueMakesSense = RowCountValueMakesSense;
                    }

                    if (nestedElse is IfStatement elseIf)
                    {
                        nestedIf = elseIf.ThenStatement;
                        nestedElse = elseIf.ElseStatement;
                        elseIf.Predicate.Accept(this);
                    }
                    else
                    {
                        nestedIf = nestedElse;
                        nestedElse = null;
                    }
                }

                RowCountValueMakesSense = maxRowCountValueMakesSense;
            }
        }
    }
}
