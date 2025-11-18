using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0799", "DEFAULT_NULL_WORD")]
    internal sealed class DefaultNullAsStringRule : AbstractRule
    {
        private static readonly char[] TrimmedBrackets = new char[] { '{', '}', '[', ']', '-', '=', '\'', '"', '<', '>', ' ' };
        private static readonly string SearchedWord = "NULL";

        public DefaultNullAsStringRule() : base()
        {
        }

        public override void Visit(DefaultConstraintDefinition node) => ValidateExpression(node.Expression);

        // This also covers parameter definitions
        // TODO : not sure if this is a good place to analyze variable defaults.
        public override void Visit(DeclareVariableElement node) => ValidateExpression(node.Value);

        private static bool IsValidDefaultText(string defaultText)
        {
            if (string.IsNullOrWhiteSpace(defaultText))
            {
                return true;
            }

            defaultText = defaultText.Trim(TrimmedBrackets);

            return !string.Equals(defaultText, SearchedWord, StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateExpression(ScalarExpression node)
        {
            if (node is null)
            {
                return;
            }

            var value = node.ExtractScalarExpression();
            if (value != null && value is StringLiteral s)
            {
                if (!IsValidDefaultText(s.Value))
                {
                    HandleNodeError(s);
                }
            }
        }
    }
}
