using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0722", "STATEMENT_IN_PARENTHESIS")]
    internal sealed class StandaloneStatementParenthesisRule : AbstractRule
    {
        public StandaloneStatementParenthesisRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new StatementVisitor(ViolationHandlerPerLine));

        private class StatementVisitor : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> ignoredStatements = new List<TSqlFragment>();
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

                while (i < n && ScriptDomExtension.IsSkippableTokens(node.ScriptTokenStream[i].TokenType))
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
