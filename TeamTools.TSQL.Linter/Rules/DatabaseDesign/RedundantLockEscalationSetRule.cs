using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0758", "REDUNDANT_TABLE_LOCK_ESCALATION")]
    internal sealed class RedundantLockEscalationSetRule : ScriptAnalysisServiceConsumingRule
    {
        public RedundantLockEscalationSetRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName)
            || !(mainObject.ObjectDefinitionNode is CreateTableStatement))
            {
                return;
            }

            var tableVisitor = GetService<TableIndicesVisitor>(node);

            bool tableIsPartitioned = tableVisitor.Table != null
                && tableVisitor.OnFileGroupOrPartitionScheme != null
                && tableVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count > 0;

            node.Accept(new LockEscalationVisitor(tableIsPartitioned, ViolationHandlerWithMessage));
        }

        private class LockEscalationVisitor : TSqlFragmentVisitor
        {
            private readonly bool isPartitioned;
            private readonly Action<TSqlFragment, string> callback;
            private LockEscalationTableOption lastSet;

            public LockEscalationVisitor(bool isPartitioned, Action<TSqlFragment, string> callback)
            {
                this.isPartitioned = isPartitioned;
                this.callback = callback;
            }

            public override void Visit(AlterTableSetStatement node)
            {
                var lockOption = DetectLockEscalation(node.Options);
                if (lockOption is null)
                {
                    return;
                }

                if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    callback(lockOption, "setting it for temp table is unexpected");
                    return;
                }

                if (lockOption.Value == LockEscalationMethod.Table)
                {
                    callback(lockOption, "'TABLE' is the default level");
                }
                else if (!isPartitioned && lockOption.Value == LockEscalationMethod.Auto)
                {
                    callback(lockOption, "'AUTO' is only useful for partitined tables");
                }

                if (lastSet != null)
                {
                    callback(lockOption, "it was already set in the script");
                }

                lastSet = lockOption;
            }

            private static LockEscalationTableOption DetectLockEscalation(IList<TableOption> options)
            {
                int n = options.Count;
                for (int i = 0; i < n; i++)
                {
                    if (options[i] is LockEscalationTableOption l)
                    {
                        return l;
                    }
                }

                return default;
            }
        }
    }
}
