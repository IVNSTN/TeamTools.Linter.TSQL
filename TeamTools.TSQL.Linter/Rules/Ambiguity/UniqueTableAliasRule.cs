using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0169", "NONUNIQUE_TABLE_ALIAS")]
    internal sealed class UniqueTableAliasRule : AbstractRule
    {
        public UniqueTableAliasRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var checkedQueries = new List<KeyValuePair<int, int>>();
            var tableAliasesVisitor = new TableAliasVisitor(
                checkedQueries,
                (i, aliases, alias, name) =>
                {
                    if (!aliases.ContainsKey(alias))
                    {
                        return;
                    }

                    HandleNodeError(i);
                }, true);
            node.AcceptChildren(tableAliasesVisitor);
        }
    }
}
