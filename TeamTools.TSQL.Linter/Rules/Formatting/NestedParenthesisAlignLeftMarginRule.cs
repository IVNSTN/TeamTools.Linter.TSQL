using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0255", "NESTED_PARENTHESIS_LEFT_MARGIN")]
    internal sealed class NestedParenthesisAlignLeftMarginRule : AbstractRule
    {
        public NestedParenthesisAlignLeftMarginRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var parenthesisParser = new ParenthesisParser(node);
            parenthesisParser.Parse();

            foreach (var i in parenthesisParser.OpenParenthesises)
            {
                if (i.Value.ParentTokenIndex == -1)
                {
                    continue;
                }

                if (ValidateParenthesisAlign(node, i.Value, parenthesisParser.OpenParenthesises[i.Value.ParentTokenIndex]))
                {
                    continue;
                }

                HandleTokenError(node.ScriptTokenStream[i.Value.OpenTokenIndex]);
            }
        }

        private bool ValidateParenthesisAlign(TSqlFragment node, ParenthesisInfo nested, ParenthesisInfo parent)
        {
            if (node.ScriptTokenStream[parent.OpenTokenIndex].Column > node.ScriptTokenStream[parent.CloseTokenIndex].Column)
            {
                // cte
                if (node.ScriptTokenStream[nested.OpenTokenIndex].Column <= node.ScriptTokenStream[parent.CloseTokenIndex].Column)
                {
                    return false;
                }
            }
            else if (node.ScriptTokenStream[nested.OpenTokenIndex].Column <= node.ScriptTokenStream[parent.OpenTokenIndex].Column)
            {
                return false;
            }

            if (node.ScriptTokenStream[nested.CloseTokenIndex].Line == node.ScriptTokenStream[parent.CloseTokenIndex].Line
            || node.ScriptTokenStream[nested.CloseTokenIndex].Line == node.ScriptTokenStream[nested.OpenTokenIndex].Line)
            {
                // in one-line expressions nested closing parenthesis cannot be to the right of parent one
                return true;
            }

            if (node.ScriptTokenStream[parent.CloseTokenIndex].Column > node.ScriptTokenStream[parent.OpenTokenIndex].Column)
            {
                // if parent () is broken then what can we say about nested
                return true;
            }

            if (node.ScriptTokenStream[nested.CloseTokenIndex].Column <= node.ScriptTokenStream[parent.CloseTokenIndex].Column)
            {
                return false;
            }

            return true;
        }
    }
}
