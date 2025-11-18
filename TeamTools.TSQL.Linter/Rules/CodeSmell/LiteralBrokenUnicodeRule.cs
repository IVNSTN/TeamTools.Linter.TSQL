using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0792", "BROKEN_UNICODE_LITERAL")]
    internal sealed class LiteralBrokenUnicodeRule : AbstractRule
    {
        // TODO : take from config
        private const int VarCharCodePage = 1251;

        public LiteralBrokenUnicodeRule() : base()
        {
        }

        public override void Visit(StringLiteral node)
        {
            if (node.IsNational || string.IsNullOrWhiteSpace(node.Value))
            {
                return;
            }

            // TODO : If the file was not in UTF8, e.g. in 1251, then valid 1251 characters are
            // falsely detected as unicode symbols. Not sure if this is supposed to be "fixed"
            // since the whole code expects UTF8+BOM SQL source files encoding.
            if (node.Value.ContainsNationalCharacter(VarCharCodePage))
            {
                HandleNodeError(node);
            }
        }
    }
}
