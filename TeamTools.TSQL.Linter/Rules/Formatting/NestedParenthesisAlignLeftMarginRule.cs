using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0255", "NESTED_PARENTHESIS_LEFT_MARGIN")]
    internal sealed class NestedParenthesisAlignLeftMarginRule : ScriptAnalysisServiceConsumingRule
    {
        public NestedParenthesisAlignLeftMarginRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var parenthesisParser = GetService<ParenthesisParser>(node);

            foreach (var i in parenthesisParser.OpenParenthesises.Values)
            {
                if (i.ParentTokenIndex >= 0
                && !ValidateParenthesisAlign(node, i, parenthesisParser.OpenParenthesises[i.ParentTokenIndex]))
                {
                    HandleTokenError(node.ScriptTokenStream[i.OpenTokenIndex]);
                }
            }
        }

        private static bool ValidateParenthesisAlign(TSqlFragment node, ParenthesisInfo nested, ParenthesisInfo parent)
        {
            var parentOpen = node.ScriptTokenStream[parent.OpenTokenIndex];
            var parentClose = node.ScriptTokenStream[parent.CloseTokenIndex];
            var nestedOpen = node.ScriptTokenStream[nested.OpenTokenIndex];

            if (parentOpen.Column > parentClose.Column)
            {
                // cte
                if (nestedOpen.Column <= parentClose.Column)
                {
                    return false;
                }
            }
            else if (nestedOpen.Column <= parentOpen.Column)
            {
                return false;
            }

            var nestedClose = node.ScriptTokenStream[nested.CloseTokenIndex];

            if (nestedClose.Line == parentClose.Line
            || nestedClose.Line == nestedOpen.Line)
            {
                // in one-line expressions nested closing parenthesis cannot be to the right of parent one
                return true;
            }

            if (parentClose.Column > parentOpen.Column)
            {
                // if parent () is broken then what can we say about nested
                return true;
            }

            return nestedClose.Column > parentClose.Column;
        }
    }
}
