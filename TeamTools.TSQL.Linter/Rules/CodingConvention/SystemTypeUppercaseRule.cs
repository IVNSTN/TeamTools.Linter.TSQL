using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0838", "SYSTEM_TYPE_UPPERCASE")]
    internal sealed class SystemTypeUppercaseRule : AbstractRule
    {
        public SystemTypeUppercaseRule() : base()
        {
        }

        public override void Visit(DataTypeReference node)
        {
            if (node.Name is null)
            {
                // e.g. CURSOR
                ValidateName(node, node.ScriptTokenStream[node.FirstTokenIndex].Text);
            }
            else if (node.IsBuiltInType())
            {
                ValidateName(node.Name, node.Name.BaseIdentifier.Value);
            }
        }

        private void ValidateName(TSqlFragment node, string name)
        {
            if (!name.IsUpperCase())
            {
                HandleNodeError(node, name);
            }
        }
    }
}
