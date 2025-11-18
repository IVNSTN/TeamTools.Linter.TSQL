using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0976", "DROPPING_NOT_OWNED_TABLE")]
    internal sealed class DroppingOuterTempTableRule : AbstractRule
    {
        public DroppingOuterTempTableRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DetectBadDrops(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                DetectBadDrops(trg.StatementList);
            }
        }

        private void DetectBadDrops(TSqlFragment node) => node?.Accept(new DropVisitor(ViolationHandlerWithMessage));

        private class DropVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> createdTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Action<TSqlFragment, string> callback;

            public DropVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(CreateTableStatement node)
            {
                createdTables.Add(node.SchemaObjectName.GetFullName());
            }

            public override void Visit(SelectStatement node)
            {
                // select-into create tables
                if (node.Into is null)
                {
                    return;
                }

                createdTables.Add(node.Into.GetFullName());
            }

            public override void Visit(DropTableStatement node)
            {
                int n = node.Objects.Count;
                for (int i = 0; i < n; i++)
                {
                    var obj = node.Objects[i];
                    string tableName = obj.GetFullName();
                    if (!tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                    {
                        // handling temp tables only
                        continue;
                    }

                    if (createdTables.Contains(tableName))
                    {
                        continue;
                    }

                    callback(node, tableName);
                }
            }
        }
    }
}
