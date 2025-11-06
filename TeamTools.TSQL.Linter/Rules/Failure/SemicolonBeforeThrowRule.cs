using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : implement similar rule for MOVE, SEND, RECEIVE statements
    [RuleIdentity("FA0256", "SEMICOLON_BEFORE_THROW")]
    [CompatibilityLevel(SqlVersion.Sql110)]
    internal sealed class SemicolonBeforeThrowRule : AbstractRule
    {
        public SemicolonBeforeThrowRule() : base()
        {
        }

        public override void Visit(ThrowStatement node)
            => SemicolonBeforeStatementFinder.FindSemicolon(node, () => HandleNodeError(node));
    }
}
