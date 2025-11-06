using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0170", "TABLE_ALIAS_MIMICKS_OTHER_TABLE")]
    internal sealed class TableAliasMimicksOtherTableRule : AbstractRule
    {
        public TableAliasMimicksOtherTableRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            var checkedTopLevelQueries = new List<KeyValuePair<int, int>>();
            var tableAliasesVisitor = new TableAliasVisitor(
                checkedTopLevelQueries,
                (i, aliases, alias, name) =>
                {
                    if (aliases.ContainsKey(alias))
                    {
                        // assuming already checked
                        return;
                    }

                    string aliasAsPartofName = string.Concat(TSqlDomainAttributes.NamePartSeparator, alias);

                    if (name.Equals(alias, StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith(aliasAsPartofName))
                    {
                        // "mimicks" itself
                        return;
                    }

                    // given alias mimicks any of registered table names
                    // or name is the same as any of registered aliases
                    if (aliases.Where(r => r.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)
                        || r.Value.EndsWith(aliasAsPartofName)).Any())
                    {
                        HandleNodeError(i, alias);
                    }
                    else if (aliases.ContainsKey(name) || aliases.ContainsKey(GetLastNamePart(name)))
                    {
                        HandleNodeError(i, alias);
                    }
                });
            node.Accept(tableAliasesVisitor);
        }

        public override void Visit(DataModificationSpecification node)
        {
            var checkedTopLevelQueries = new List<KeyValuePair<int, int>>();
            var tableAliasesVisitor = new TableAliasVisitor(
                checkedTopLevelQueries,
                (i, aliases, alias, name) =>
                {
                    if (aliases.ContainsKey(alias))
                    {
                        // assuming already checked
                        return;
                    }

                    string aliasAsPartofName = string.Concat(TSqlDomainAttributes.NamePartSeparator, alias);

                    if (name.Equals(alias, StringComparison.OrdinalIgnoreCase) || name.EndsWith(aliasAsPartofName))
                    {
                        // "mimicks" itself
                        return;
                    }

                    // given alias mimicks any of registered table names
                    // or name is the same as any of registered aliases
                    if (aliases.Where(r => r.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)
                        || r.Value.EndsWith(aliasAsPartofName)).Any())
                    {
                        HandleNodeError(i, alias);
                    }
                    else if (aliases.ContainsKey(name) || aliases.ContainsKey(GetLastNamePart(name)))
                    {
                        HandleNodeError(i, alias);
                    }
                });
            node.Accept(tableAliasesVisitor);
        }

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
    }
}
