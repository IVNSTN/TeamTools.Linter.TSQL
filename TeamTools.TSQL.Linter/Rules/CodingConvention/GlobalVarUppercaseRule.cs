using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0839", "GLOBAL_VAR_UPPERCASE")]
    internal sealed class GlobalVarUppercaseRule : AbstractRule
    {
        public GlobalVarUppercaseRule() : base()
        {
        }

        public override void Visit(GlobalVariableExpression node)
        {
            if (!node.Name.IsUpperCase())
            {
                HandleNodeError(node, node.Name);
            }
        }
    }
}
