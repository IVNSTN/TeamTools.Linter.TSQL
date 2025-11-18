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
        private readonly OutputVisitor outputVisitor;

        public ReferencingInsertedDeletedInOutputInTriggerRule() : base()
        {
            outputVisitor = new OutputVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                trg.StatementList?.AcceptChildren(outputVisitor);
            }
        }

        private sealed class OutputVisitor : VisitorWithCallback
        {
            public OutputVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DataModificationSpecification node)
            {
                if (node.OutputClause is null && node.OutputIntoClause is null)
                {
                    return;
                }

                var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var checkedTopLevelQueries = new List<Tuple<int, int>>();
                var aliasVisitor = new TableAliasVisitor(checkedTopLevelQueries, aliases, null);

                node.Accept(aliasVisitor);

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
                Callback((TSqlFragment)node.OutputClause ?? node.OutputIntoClause);
            }
        }
    }
}
