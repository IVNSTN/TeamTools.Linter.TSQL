using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0960", "UNRELIABLE_TYPE_CHECK")]
    internal sealed class UnreliableTypeCheckRule : AbstractRule
    {
        private static readonly ICollection<string> ForbiddenFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static UnreliableTypeCheckRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            ForbiddenFunctions.Add("ISNUMERIC");
            ForbiddenFunctions.Add("ISDATE");
        }

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
