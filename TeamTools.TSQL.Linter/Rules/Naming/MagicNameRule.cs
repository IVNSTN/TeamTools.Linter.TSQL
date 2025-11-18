using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0271", "MAGIC_@@_##_NAME")]
    internal sealed class MagicNameRule : AbstractRule
    {
        public MagicNameRule() : base()
        {
        }

        public override void Visit(DeclareTableVariableBody node) => DoValidateName(node.VariableName);

        public override void Visit(DeclareVariableElement node) => DoValidateName(node.VariableName);

        public override void Visit(CreateTableStatement node) => DoValidateName(node.SchemaObjectName.BaseIdentifier);

        private static bool IsMagicName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            int n = name.Length;
            // @SomeVar - last char is less likely to be '@'
            for (int i = n - 1; i >= 0; i--)
            {
                char c = name[i];
                if (c != TSqlDomainAttributes.VariablePrefixChar
                && c != TSqlDomainAttributes.TempTablePrefixChar)
                {
                    return false;
                }
            }

            return true;
        }

        private void DoValidateName(Identifier name)
        {
            if (name is null)
            {
                // in inline-table function output definition has no name
                return;
            }

            if (!IsMagicName(name.Value))
            {
                return;
            }

            HandleNodeError(name);
        }
    }
}
