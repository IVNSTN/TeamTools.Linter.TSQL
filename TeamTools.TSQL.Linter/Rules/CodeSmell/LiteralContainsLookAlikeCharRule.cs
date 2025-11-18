using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // See also CommentContainsLookAlikeCharRule, AlphabetMixInIdentifierRule
    [RuleIdentity("CS0834", "LITERAL_LOOK_ALIKE_CHAR")]
    internal sealed class LiteralContainsLookAlikeCharRule : AbstractRule
    {
        public LiteralContainsLookAlikeCharRule() : base()
        {
        }

        // TODO : utilize ExpressionEvaluator here?
        public override void Visit(StringLiteral node) => LookAlikeCharDetector.ValidateChars(node.Value, node.StartLine, node.StartColumn, ViolationHandlerPerLine);
    }
}
