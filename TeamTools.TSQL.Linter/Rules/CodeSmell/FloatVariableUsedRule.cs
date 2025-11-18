using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0993", "FLOAT_VARIABLE")]
    internal sealed class FloatVariableUsedRule : AbstractRule
    {
        private static readonly HashSet<string> FloatNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FLOAT",
            "REAL",
        };

        public FloatVariableUsedRule() : base()
        {
        }

        public override void Visit(DeclareVariableElement node)
        {
            if (node.DataType?.Name is null)
            {
                // e.g. CURSOR
                return;
            }

            if (FloatNames.Contains(node.DataType.Name.BaseIdentifier.Value))
            {
                HandleNodeError(node.DataType);
            }
        }
    }
}
