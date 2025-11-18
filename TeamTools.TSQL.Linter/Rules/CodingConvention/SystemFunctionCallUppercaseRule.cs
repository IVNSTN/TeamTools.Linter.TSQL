using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0202", "SYSTEM_FUNC_UPPER")]
    internal sealed class SystemFunctionCallUppercaseRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private Dictionary<string, string> systemFunctions;

        public SystemFunctionCallUppercaseRule() : base()
        {
        }

        public override void Visit(PrimaryExpression node)
        {
            Debug.Assert(systemFunctions.Count > 0, "systemFunctions not loaded");

            // some parser bug
            if (node.FirstTokenIndex < 0)
            {
                return;
            }

            TSqlParserToken token = node.ScriptTokenStream[node.FirstTokenIndex];

            if (!IsPossibleFunctionCall(token.TokenType))
            {
                return;
            }

            if (!systemFunctions.TryGetValue(token.Text, out var correctSpelling))
            {
                // no function with such name
                return;
            }

            if (string.Equals(token.Text, correctSpelling, StringComparison.Ordinal))
            {
                // text is in upper case
                return;
            }

            // If this is a function call then next char must be left parenthesis
            // otherwise it could be an identifier which only looks like a function call
            if (IsFollowedByParenthesis(node))
            {
                HandleNodeError(node, token.Text);
            }
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            systemFunctions = data.Functions
                .ToDictionary(fn => fn.Key, fn => fn.Key.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsPossibleFunctionCall(TSqlTokenType tokenType)
        {
            return tokenType == TSqlTokenType.Identifier
                || tokenType == TSqlTokenType.Left
                || tokenType == TSqlTokenType.Right
                || tokenType == TSqlTokenType.Coalesce
                || tokenType == TSqlTokenType.NullIf
                || tokenType == TSqlTokenType.Convert
                || tokenType == TSqlTokenType.TryConvert
                || tokenType == TSqlTokenType.CurrentTimestamp
                || tokenType == TSqlTokenType.SystemUser;
        }

        private static bool IsFollowedByParenthesis(TSqlFragment node)
        {
            int n = node.ScriptTokenStream.Count;

            for (int i = node.FirstTokenIndex + 1; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (!ScriptDomExtension.IsSkippableTokens(token.TokenType))
                {
                    return token.TokenType == TSqlTokenType.LeftParenthesis;
                }
            }

            return false;
        }
    }
}
