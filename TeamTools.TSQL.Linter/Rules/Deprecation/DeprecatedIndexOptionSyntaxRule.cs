using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0741", "INDEX_OPTION_SYNTAX_DEPRECATED")]
    [IndexRule]
    internal sealed class DeprecatedIndexOptionSyntaxRule : AbstractRule
    {
        public DeprecatedIndexOptionSyntaxRule() : base()
        {
        }

        public override void Visit(CreateIndexStatement node) => ValidateOptionsSyntax(node.IndexOptions);

        public override void Visit(IndexDefinition node) => ValidateOptionsSyntax(node.IndexOptions);

        private void ValidateOptionsSyntax(IList<IndexOption> indexOptions)
        {
            if (indexOptions.Count == 0)
            {
                return;
            }

            int i = indexOptions[0].FirstTokenIndex - 1;
            bool parenthesisFound = false;
            var stream = indexOptions[0].ScriptTokenStream;

            while (i > 0 && stream[i].TokenType != TSqlTokenType.With && !parenthesisFound)
            {
                parenthesisFound = stream[i].TokenType == TSqlTokenType.LeftParenthesis;
                i--;
            }

            if (!parenthesisFound)
            {
                HandleNodeError(indexOptions[0]);
            }
        }
    }
}
