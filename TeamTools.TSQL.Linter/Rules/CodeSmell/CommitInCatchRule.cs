using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0151", "COMMIT_IN_CATCH")]
    internal sealed class CommitInCatchRule : AbstractRule
    {
        public CommitInCatchRule() : base()
        {
        }

        public override void Visit(TryCatchStatement node)
            => TSqlViolationDetector.DetectFirst<CommitVisitor>(node.CatchStatements, HandleNodeError);

        private class CommitVisitor : TSqlViolationDetector
        {
            public override void Visit(CommitTransactionStatement node) => MarkDetected(node);
        }
    }
}
