using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0272", "BOF_REDUNDANT_WHITESPACE")]
    internal sealed class NewLineAtBeginOfFileRule : AbstractRule
    {
        public NewLineAtBeginOfFileRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int i = node.FirstTokenIndex;
            if (i < 0)
            {
                return;
            }

            if (node.ScriptTokenStream[i].TokenType != TSqlTokenType.WhiteSpace)
            {
                return;
            }

            HandleFileError();
        }
    }
}
