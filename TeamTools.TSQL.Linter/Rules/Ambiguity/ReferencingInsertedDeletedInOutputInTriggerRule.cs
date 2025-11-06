using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0162", "OUTPUT_IN_TRIGGER_AMBIGUOUS_INSDEL")]
    [TriggerRule]
    internal sealed class ReferencingInsertedDeletedInOutputInTriggerRule : AbstractRule
    {
        public ReferencingInsertedDeletedInOutputInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        {
            var outputVisitor = new OutputVisitor();
            node.AcceptChildren(outputVisitor);

            if (outputVisitor.BadOutputClauses.Count == 0)
            {
                return;
            }

            foreach (var outputClause in outputVisitor.BadOutputClauses)
            {
                HandleNodeError(outputClause);
            }
        }

        private class OutputVisitor : TSqlFragmentVisitor
        {
            private readonly IList<TSqlFragment> badOutputClauses = new List<TSqlFragment>();

            public IList<TSqlFragment> BadOutputClauses => badOutputClauses;

            public override void Visit(DataModificationSpecification node)
            {
                var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var checkedTopLevelQueries = new List<KeyValuePair<int, int>>();
                var aliasVisitor = new TableAliasVisitor(checkedTopLevelQueries, aliases, null);

                if (null != node.OutputClause)
                {
                    node.Accept(aliasVisitor);
                }
                else if (null != node.OutputIntoClause)
                {
                    node.Accept(aliasVisitor);
                }
                else
                {
                    return;
                }

                if (aliases.Count == 0)
                {
                    return;
                }

                if (!aliases.ContainsKey(TSqlDomainAttributes.TriggerSystemTables.Inserted)
                && !aliases.ContainsKey(TSqlDomainAttributes.TriggerSystemTables.Deleted)
                && !aliases.ContainsValue(TSqlDomainAttributes.TriggerSystemTables.Inserted)
                && !aliases.ContainsValue(TSqlDomainAttributes.TriggerSystemTables.Deleted))
                {
                    return;
                }

                // if INSERTED/DELETED used in OUTPUT together with INSERTED/DELETED in FROM inside TRIGGER make server dump
                if (node.OutputClause != null)
                {
                    BadOutputClauses.Add(node.OutputClause);
                }
                else
                {
                    BadOutputClauses.Add(node.OutputIntoClause);
                }
            }
        }
    }
}
