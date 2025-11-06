using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0181", "IDENTITY_FUNCTION")]
    internal sealed class IdentityFunctionRule : AbstractRule
    {
        private const string IdentityVarName = "@@IDENTITY";

        public IdentityFunctionRule() : base()
        {
        }

        public override void Visit(GlobalVariableExpression node)
        {
            if (!node.Name.Equals(IdentityVarName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
