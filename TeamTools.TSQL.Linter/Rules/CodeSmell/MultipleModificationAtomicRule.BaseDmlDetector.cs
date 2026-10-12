using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class MultipleModificationAtomicRule
    {
        internal abstract class BaseDmlDetector : TSqlFragmentVisitor
        {
            private readonly string violationMessage;

            protected BaseDmlDetector(Action<TSqlFragment, string> callback, string violationMessage) : base()
            {
                Callback = callback;
                this.violationMessage = violationMessage;
            }

            protected int DmlCount { get; set; }

            protected Action<TSqlFragment, string> Callback { get; }

            public override void Visit(DataModificationStatement node)
            {
                if (CanDmlBeIgnored(node))
                {
                    return;
                }

                if (DmlCount == 1)
                {
                    // Reporting once per bad block on the second statement
                    Callback(node, violationMessage);
                }

                DmlCount++;
            }

            // DML statements in CATCH are usually applied out of general transaction
            // thus ignoring them
            public override void ExplicitVisit(TryCatchStatement node)
            {
                node.TryStatements.AcceptChildren(this);
            }

            // ExplicitVisit to avoid visiting all nested elements sequentially
            // instead they are processed in tree-like manner
            public override void ExplicitVisit(IfStatement node) => WalkAllIfElseBranches(node);

            protected abstract void WalkAllIfElseBranches(IfStatement node);

            protected virtual bool CanDmlBeIgnored(DataModificationStatement node)
            {
                // table variables do not participate in user-defined transactions
                // temporary tables can be ignored
                return RealTablesDetector.IsNoRealTableDml(node);
            }
        }
    }
}
