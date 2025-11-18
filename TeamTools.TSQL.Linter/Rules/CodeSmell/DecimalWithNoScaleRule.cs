using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0821", "DECIMAL_WITHOUT_DECIMALS")]
    internal sealed class DecimalWithNoScaleRule : AbstractRule
    {
        // TODO : Move to Evaluator after implementing decimals support
        private static readonly HashSet<string> DecimalTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // FLOAT and REAL have only one parameter, you cannot specify decimal points for them.
            "DECIMAL",
            "NUMERIC",
        };

        public DecimalWithNoScaleRule() : base()
        {
        }

        public override void Visit(SqlDataTypeReference node)
        {
            // Note, types like CURSOR have no Name provided with the parser
            if (!DecimalTypes.Contains(node.Name?.BaseIdentifier.Value))
            {
                return;
            }

            // default scale is 0
            if (node.Parameters.Count == 0)
            {
                HandleNodeError(node.Name);
                return;
            }

            // scale is the second param, e.g. DECIMAL(18,3)
            if (node.Parameters.Count == 2
            && int.TryParse(node.Parameters[1].Value, out int scale)
            && scale == 0)
            {
                HandleNodeError(node.Parameters[1]);
            }
        }
    }
}
