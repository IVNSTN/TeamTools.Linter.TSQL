using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0409", "DEPRECATED_RAISERROR_SYNTAX")]
    [CompatibilityLevel(SqlVersion.Sql80, SqlVersion.Sql100)]
    internal sealed class DeprecatedRaisErrorSyntaxRule : AbstractRule
    {
        public DeprecatedRaisErrorSyntaxRule() : base()
        {
        }

        public override void Visit(RaiseErrorLegacyStatement node) => HandleNodeError(node);
    }
}
