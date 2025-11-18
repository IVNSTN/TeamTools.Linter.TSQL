using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0960", "UNRELIABLE_TYPE_CHECK")]
    internal sealed class UnreliableTypeCheckRule : AbstractRule
    {
        // TODO : consolidate all the metadata about known built-in functions
        private static readonly HashSet<string> ForbiddenFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ISDATE",
            "ISNUMERIC",
        };

        public UnreliableTypeCheckRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (!ForbiddenFunctions.Contains(node.FunctionName.Value))
            {
                return;
            }

            HandleNodeError(node, node.FunctionName.Value);
        }
    }
}
