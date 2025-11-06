using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0722", "STATEMENT_IN_PARENTHESIS")]
    internal sealed class StandaloneStatementParenthesisRule : AbstractRule
    {
        private static readonly ICollection<TSqlTokenType> SkippedTokens;

        // TODO : this is a little similar to GarbageBeforeSemicolonRule
        static StandaloneStatementParenthesisRule()
        {
            SkippedTokens = new List<TSqlTokenType>
            {
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.SingleLineComment,
                TSqlTokenType.MultilineComment,
            };
        }

        public StandaloneStatementParenthesisRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
            => node.Accept(new StatementVisitor(HandleLineError));

        private class StatementVisitor : TSqlFragmentVisitor
        {
            private readonly ICollection<TSqlFragment> ignoredStatements = new List<TSqlFragment>();
            private readonly Action<int, int, string> callback;

            public StatementVisitor(Action<int, int, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(CreateFunctionStatement node)
            {
                if (node.ReturnType is SelectFunctionReturnType ret)
                {
                    ignoredStatements.Add(ret.SelectStatement);
                }
            }

            public override void Visit(TSqlStatement node)
            {
                if (ignoredStatements.Contains(node))
                {
                    return;
                }

                int i = node.FirstTokenIndex;
                int n = node.LastTokenIndex;

                while (i < n && SkippedTokens.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    // looking for open parenthesis
                    i++;
                }

                var firstToken = node.ScriptTokenStream[i];

                if (firstToken.TokenType == TSqlTokenType.LeftParenthesis)
                {
                    callback(firstToken.Line, firstToken.Column, default);
                }
            }
        }
    }
}
