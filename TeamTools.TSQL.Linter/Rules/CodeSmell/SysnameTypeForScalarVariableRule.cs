using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0146", "SYSNAME_FOR_SCALAR_VAR")]
    internal class SysnameTypeForScalarVariableRule : AbstractRule
    {
        public SysnameTypeForScalarVariableRule() : base()
        {
        }

        public override void Visit(DeclareVariableElement node) => ValidateType(node.DataType);

        public override void Visit(ScalarFunctionReturnType node) => ValidateType(node.DataType);

        private void ValidateType(DataTypeReference dataType)
        {
            if (dataType?.Name?.BaseIdentifier.Value.Equals("SYSNAME", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                HandleNodeError(dataType);
            }
        }
    }
}
