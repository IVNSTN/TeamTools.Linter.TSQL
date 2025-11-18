using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0157", "LOGIN_BASED_FLOW")]
    [SecurityRule]
    internal sealed class LoginBasedCodeFlowRule : AbstractRule
    {
        private readonly LoginNameVisitor visitor;

        // TODO : very similar to HostNameBasedCodeFlowRule
        public LoginBasedCodeFlowRule() : base()
        {
            visitor = new LoginNameVisitor(ViolationHandler);
        }

        // TODO : avoid double-visiting of nested expressions
        public override void Visit(BooleanExpression node)
        {
            if (node is BooleanParenthesisExpression || node is BooleanBinaryExpression)
            {
                // nested elements are boolean expressions too
                // and they will be visited explicitly
                return;
            }

            node.Accept(visitor);
        }

        private class LoginNameVisitor : VisitorWithCallback
        {
            private static readonly HashSet<TSqlTokenType> TokenTypes = new HashSet<TSqlTokenType>
            {
                TSqlTokenType.SessionUser,
                TSqlTokenType.CurrentUser,
                TSqlTokenType.SystemUser,
                TSqlTokenType.Identifier,
            };

            private static readonly HashSet<string> UserFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CURRENT_USER",
                "ORIGINAL_LOGIN",
                "SESSION_USER",
                "SUSER_ID",
                "SUSER_NAME",
                "SUSER_SID",
                "SUSER_SNAME",
                "SYSTEM_USER",
                "USER",
                "USER_ID",
                "USER_NAME",
            };

            public LoginNameVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(PrimaryExpression node)
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                if (IsUserReferenceToken(node.ScriptTokenStream[node.FirstTokenIndex]))
                {
                    Callback(node);
                }
            }

            private static bool IsUserReferenceToken(TSqlParserToken token)
                => TokenTypes.Contains(token.TokenType) && UserFunctions.Contains(token.Text);
        }
    }
}
