using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(ProcedureStatementBody node)
            => node.AcceptChildren(new DropVisitor(HandleNodeError));

        public override void Visit(TriggerStatementBody node)
            => node.AcceptChildren(new DropVisitor(HandleNodeError));

        private class DropVisitor : TSqlFragmentVisitor
        {
            private readonly List<string> createdTables = new List<string>();
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
                foreach (var obj in node.Objects)
                {
                    string tableName = obj.GetFullName();
                    if (!tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                    {
                        // handling temp tables only
                        continue;
                    }

                    if (createdTables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    callback(node, tableName);
                }
            }
        }
    }
}
