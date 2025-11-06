using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [Obsolete("Looks like a dup of " + nameof(RedundantFunctionArgumentRule))]
    [RuleIdentity("RD0194", "REDUNDANT_COALESCE_ARGUMENT")]
    internal sealed class RedundantCoalesceArgumentRule : AbstractRule
    {
        public RedundantCoalesceArgumentRule() : base()
        {
        }

        public override void Visit(CoalesceExpression node)
        {
            bool nextIsRedundant = false;

            foreach (var expr in node.Expressions)
            {
                if (nextIsRedundant)
                {
                    HandleNodeError(expr);
                }
                else
                if (expr is NullLiteral)
                {
                    HandleNodeError(expr);
                }
                else if (expr is Literal)
                {
                    nextIsRedundant = true;
                }
            }
        }
    }
}
