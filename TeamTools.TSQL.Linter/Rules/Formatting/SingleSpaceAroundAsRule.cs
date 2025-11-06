using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0275", "AS_WITH_SPACES")]
    internal sealed class SingleSpaceAroundAsRule : AbstractRule
    {
        private static readonly IList<TSqlTokenType> SeparatorTokens;

        private static readonly Regex AliasPatern = new Regex(
            "^[\\s]{1}AS[\\s]{1}$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static SingleSpaceAroundAsRule()
        {
            SeparatorTokens = new List<TSqlTokenType>
            {
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.SingleLineComment,
                TSqlTokenType.MultilineComment,
            };
        }

        public SingleSpaceAroundAsRule() : base()
        {
        }

        public override void Visit(TableReferenceWithAlias node) => ValidateAsSpaces(node.Alias);

        public override void Visit(SelectScalarExpression node)
        {
            // no alias or 'alias = expression' syntax used
            if (node.ColumnName is null
            || node.ColumnName.FirstTokenIndex < node.Expression.FirstTokenIndex)
            {
                return;
            }

            ValidateAsSpaces(node.ColumnName);
        }

        private void ValidateAsSpaces(TSqlFragment node)
        {
            if (node is null)
            {
                return;
            }

            int i = node.FirstTokenIndex - 1;
            bool asDetected = false;
            StringBuilder fragmentText = new StringBuilder();

            while (i > 0 && (SeparatorTokens.Contains(node.ScriptTokenStream[i].TokenType) || node.ScriptTokenStream[i].TokenType == TSqlTokenType.As))
            {
                fragmentText.Insert(0, node.ScriptTokenStream[i].Text);
                asDetected = asDetected || (node.ScriptTokenStream[i].TokenType == TSqlTokenType.As);
                i--;
            }

            // no reason to count spaces "around AS" if no AS found
            if (asDetected && !AliasPatern.IsMatch(fragmentText.ToString()))
            {
                HandleNodeError(node);
            }
        }
    }
}
