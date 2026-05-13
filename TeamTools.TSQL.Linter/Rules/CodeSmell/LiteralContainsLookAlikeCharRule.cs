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

        public override void ExplicitVisit(LikePredicate node)
        {
            if (!(node.SecondExpression is StringLiteral s))
            {
                return;
            }

            // TODO : Check other parts of the pattern except the [] contents.
            // Pay attention to violation positioning after string adaptation.
            if (s.Value.Contains("["))
            {
                return;
            }

            s.Accept(this);
        }
    }
}
