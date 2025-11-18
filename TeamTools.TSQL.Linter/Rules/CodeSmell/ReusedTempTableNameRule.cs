using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0830", "NAME_REUSED_TEMP_TABLE")]
    internal sealed class ReusedTempTableNameRule : AbstractRule
    {
        public ReusedTempTableNameRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            node.AcceptChildren(new TempTableVisitor(ViolationHandlerWithMessage));
        }

        private sealed class TempTableVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Action<TSqlFragment, string> callback;

            public TempTableVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            // If table creations are divided by if-else branches
            // then the issue is still actual
            public override void Visit(CreateTableStatement node)
            {
                string tableName = node.SchemaObjectName.BaseIdentifier.Value;
                if (!tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    // we need temp tables only
                    return;
                }

                if (!usedNames.Add(tableName))
                {
                    callback.Invoke(node.SchemaObjectName, tableName);
                }
            }
        }
    }
}
