using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0758", "REDUNDANT_TABLE_LOCK_ESCALATION")]
    internal sealed class RedundantLockEscalationSetRule : AbstractRule
    {
        public RedundantLockEscalationSetRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var mainObjectDetector = new MainScriptObjectDetector();
            node.Accept(mainObjectDetector);

            if (string.IsNullOrEmpty(mainObjectDetector.ObjectFullName)
            || !(mainObjectDetector.ObjectDefinitionNode is CreateTableStatement))
            {
                return;
            }

            if (mainObjectDetector.ObjectFullName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                HandleNodeError(node, "setting it for temp table is unexpected");
                return;
            }

            var tableVisitor = new TableIndicesVisitor();
            node.Accept(tableVisitor);

            bool tableIsPartitioned = tableVisitor.Table != null
                && tableVisitor.OnFileGroupOrPartitionScheme != null
                && tableVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count > 0;

            node.Accept(new LockEscalationVisitor(tableIsPartitioned, HandleNodeError));
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
                var lockOption = node.Options.OfType<LockEscalationTableOption>().FirstOrDefault();
                if (lockOption is null)
                {
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
        }
    }
}
