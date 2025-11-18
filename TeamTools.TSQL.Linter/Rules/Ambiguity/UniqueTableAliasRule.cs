using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0169", "NONUNIQUE_TABLE_ALIAS")]
    internal sealed class UniqueTableAliasRule : AbstractRule
    {
        private readonly Action<TSqlFragment, IDictionary<string, string>, string, string> validator;

        public UniqueTableAliasRule() : base()
        {
            validator = new Action<TSqlFragment, IDictionary<string, string>, string, string>((i, aliases, alias, name) =>
            {
                if (!aliases.ContainsKey(alias))
                {
                    return;
                }

                HandleNodeError(i);
            });
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var tableAliasesVisitor = new TableAliasVisitor(
                new List<Tuple<int, int>>(),
                validator,
                true);
            node.AcceptChildren(tableAliasesVisitor);
        }
    }
}
