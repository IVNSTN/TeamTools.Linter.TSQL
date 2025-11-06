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

        public override void Visit(DeclareTableVariableBody node)
        {
            if (node.VariableName == null)
            {
                // in inline-table function output definition has no name
                return;
            }

            if (!node.VariableName.Value.Replace(TSqlDomainAttributes.VariablePrefix, "").Equals(""))
            {
                return;
            }

            HandleNodeError(node);
        }

        public override void Visit(DeclareVariableElement node)
        {
            if (!node.VariableName.Value.Replace(TSqlDomainAttributes.VariablePrefix, "").Equals(""))
            {
                return;
            }

            HandleNodeError(node);
        }

        public override void Visit(CreateTableStatement node)
        {
            if (!node.SchemaObjectName.BaseIdentifier.Value.Replace(TSqlDomainAttributes.TempTablePrefix, "").Equals(""))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
