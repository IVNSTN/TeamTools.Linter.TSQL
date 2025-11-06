using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Ambiguous column reference detector.
    /// </summary>
    [RuleIdentity("AM0935", "AMBIGUOUS_COL_SOURCE")]
    internal sealed partial class AmbiguousColumnBelongingRule : AbstractRule
    {
        private const int MaxIssuesPerBatch = 5;

        // TODO : refactoring and simplification needed
        public AmbiguousColumnBelongingRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var missingAliases = new List<MultiPartIdentifier>();
            var validator = new QueryValidator(missingAliases.AddRange);
            node.AcceptChildren(validator);

            if (!missingAliases.Any())
            {
                return;
            }

            var reportedIssues = missingAliases
                .OrderBy(col => col.StartLine)
                .Select(col => col.Identifiers[0].Value)
                .Distinct()
                .Take(MaxIssuesPerBatch);

            HandleNodeError(missingAliases.First(), string.Join(", ", reportedIssues));
        }
    }
}
