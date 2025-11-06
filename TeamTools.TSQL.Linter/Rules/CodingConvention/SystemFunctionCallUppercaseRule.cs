using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0202", "SYSTEM_FUNC_UPPER")]
    internal sealed class SystemFunctionCallUppercaseRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private static readonly ICollection<TSqlTokenType> TokenTypes;

        private ICollection<string> systemFunctions;

        static SystemFunctionCallUppercaseRule()
        {
            TokenTypes = new SortedSet<TSqlTokenType>
            {
                TSqlTokenType.Identifier,
                TSqlTokenType.Left,
                TSqlTokenType.Right,
                TSqlTokenType.Coalesce,
                TSqlTokenType.NullIf,
                TSqlTokenType.Convert,
                TSqlTokenType.TryConvert,
                TSqlTokenType.CurrentTimestamp,
                TSqlTokenType.SystemUser,
            };
        }

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

            if (!systemFunctions.Contains(token.Text))
            {
                return;
            }

            if (!TokenTypes.Contains(token.TokenType))
            {
                return;
            }

            if (string.Equals(token.Text, token.Text.ToUpperInvariant(), StringComparison.Ordinal))
            {
                // text is in upper case
                return;
            }

            // If this is a function call then next char must be left parenthesis
            // otherwise it could be an identifier which only looks like a function call
            if (ParenthesisLocator.CheckIfFollowedByParenthesis(node))
            {
                HandleNodeError(node, token.Text);
            }
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            systemFunctions = data.Functions.Keys;
        }

        private static class ParenthesisLocator
        {
            private static readonly List<TSqlTokenType> SkippedTokens = new List<TSqlTokenType>
            {
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.SingleLineComment,
                TSqlTokenType.MultilineComment,
            };

            public static bool CheckIfFollowedByParenthesis(TSqlFragment node)
            {
                int i = node.FirstTokenIndex + 1;
                int n = node.ScriptTokenStream.Count;

                while (i < n)
                {
                    if (SkippedTokens.Contains(node.ScriptTokenStream[i].TokenType))
                    {
                        i++;
                    }
                    else if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis)
                    {
                        return true;
                    }
                    else
                    {
                        i = n;
                    }
                }

                return false;
            }
        }
    }
}
