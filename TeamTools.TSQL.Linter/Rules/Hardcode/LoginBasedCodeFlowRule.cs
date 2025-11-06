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
        // TODO : very similar to HostNameBasedCodeFlowRule
        public LoginBasedCodeFlowRule() : base()
        {
        }

        public override void Visit(BooleanExpression node)
        {
            if (node is BooleanParenthesisExpression || node is BooleanBinaryExpression)
            {
                // nested elements are boolean expressions too
                // and they will be visited explicitly
                return;
            }

            TSqlViolationDetector.DetectFirst<LoginNameVisitor>(node, HandleNodeError);
        }

        private class LoginNameVisitor : TSqlViolationDetector
        {
            private static readonly IList<TSqlTokenType> TokenTypes = new List<TSqlTokenType>
            {
                TSqlTokenType.SessionUser,
                TSqlTokenType.CurrentUser,
                TSqlTokenType.SystemUser,
                TSqlTokenType.Identifier,
            };

            private static readonly ICollection<string> UserFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ORIGINAL_LOGIN",
                "CURRENT_USER",
                "SESSION_USER",
                "SYSTEM_USER",
                "SUSER_ID",
                "SUSER_SID",
                "SUSER_NAME",
                "SUSER_SNAME",
                "USER",
                "USER_ID",
                "USER_NAME",
            };

            public override void Visit(PrimaryExpression node)
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                if (IsUserReferenceToken(node.ScriptTokenStream[node.FirstTokenIndex]))
                {
                    MarkDetected(node);
                }
            }

            private static bool IsUserReferenceToken(TSqlParserToken token)
                => TokenTypes.Contains(token.TokenType) && UserFunctions.Contains(token.Text);
        }
    }
}
