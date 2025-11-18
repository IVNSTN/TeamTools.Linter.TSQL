using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0925", "REDUNDANT_LIKE")]
    internal sealed class RedundantLikePredicateRule : AbstractRule
    {
        private static readonly HashSet<string> LikeWildcards = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "%",
            "_",
            "[",
        };

        public RedundantLikePredicateRule() : base()
        {
        }

        public override void Visit(LikePredicate node)
        {
            if (!(node.SecondExpression is StringLiteral str))
            {
                return;
            }

            string likePredicate = str.Value;

            // TODO : check if wildcard is not escaped by node.EscapeExpression
            if (!string.IsNullOrEmpty(likePredicate) && LikeWildcards.Any(w => likePredicate.Contains(w)))
            {
                return;
            }

            HandleNodeError(str);
        }
    }
}
