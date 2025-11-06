using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0227", "SELECT_IN_DECLARE")]
    internal sealed class SelectInDeclareRule : AbstractRule
    {
        public SelectInDeclareRule() : base()
        {
        }

        public override void Visit(DeclareVariableStatement node)
            => TSqlViolationDetector.DetectFirst<SelectVisitor>(node, HandleNodeError);

        private class SelectVisitor : TSqlViolationDetector
        {
            public override void Visit(QueryExpression node) => MarkDetected(node);
        }
    }
}
