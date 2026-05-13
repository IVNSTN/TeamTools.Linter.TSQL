using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0170", "TABLE_ALIAS_MIMICKS_OTHER_TABLE")]
    internal sealed class TableAliasMimicksOtherTableRule : AbstractRule
    {
        private readonly Action<TSqlFragment, IDictionary<string, string>, string, string> validationHandler;

        public TableAliasMimicksOtherTableRule() : base()
        {
            validationHandler = new Action<TSqlFragment, IDictionary<string, string>, string, string>(ValidateAlias);
        }

        public override void Visit(QuerySpecification node) => ValidateQuery(node);

        public override void Visit(DataModificationSpecification node) => ValidateQuery(node);

        private static string GetLastNamePart(string name)
        {
            int i = name.LastIndexOf(TSqlDomainAttributes.NamePartSeparator);
            if (i == -1)
            {
                return name;
            }
            else
            {
                return name.Substring(i + 1);
            }
        }

        private static bool ExistsInNames(ICollection<string> names, string alias, string aliasAsPartOfName)
        {
            foreach (var name in names)
            {
                if (name.Equals(alias, StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(aliasAsPartOfName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ValidateAlias(TSqlFragment node, IDictionary<string, string> aliases, string alias, string name)
        {
            if (aliases.Count == 0 || aliases.ContainsKey(alias))
            {
                // assuming already checked
                return;
            }

            if (name.Equals(alias, StringComparison.OrdinalIgnoreCase))
            {
                // "mimicks" itself
                return;
            }

            string aliasAsPartOfName = string.Concat(TSqlDomainAttributes.NamePartSeparator, alias);
            if (name.EndsWith(aliasAsPartOfName))
            {
                // "mimicks" itself
                return;
            }

            if (aliases.TryGetValue(GetLastNamePart(name), out string otherName)
            && !string.Equals(name, otherName, StringComparison.OrdinalIgnoreCase))
            {
                // The other alias mimicks this one name
                // and this is not the same source name reference (e.g. in subquery)
                HandleNodeError(node, alias);
            }
            else if (ExistsInNames(aliases.Values, alias, aliasAsPartOfName))
            {
                // The alias mimicks one of registered table names
                HandleNodeError(node, alias);
            }
        }

        private void ValidateQuery(TSqlFragment node)
        {
            var checkedTopLevelQueries = new List<Tuple<int, int>>();
            var tableAliasesVisitor = new TableAliasVisitor(checkedTopLevelQueries, validationHandler);
            node.Accept(tableAliasesVisitor);
        }
    }
}
