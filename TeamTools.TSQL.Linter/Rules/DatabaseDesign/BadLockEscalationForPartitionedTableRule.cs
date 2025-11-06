using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Rule implementation is very similar to RedundantLockEscalationSetRule
    [RuleIdentity("DD0759", "PARTITIONED_TABLE_LOCK_ESCALATION")]
    internal sealed class BadLockEscalationForPartitionedTableRule : AbstractRule
    {
        public BadLockEscalationForPartitionedTableRule() : base()
        {
        }

        // TODO : Clustered index definition may be separate from table definition
        public override void Visit(TSqlScript node)
        {
            var mainObjectDetector = new MainScriptObjectDetector();
            node.Accept(mainObjectDetector);

            if (string.IsNullOrEmpty(mainObjectDetector.ObjectFullName)
            || !(mainObjectDetector.ObjectDefinitionNode is CreateTableStatement)
            || mainObjectDetector.ObjectFullName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // non CREATE-TABLE script
                return;
            }

            var tableVisitor = new TableIndicesVisitor();
            node.Accept(tableVisitor);

            if (tableVisitor.Table is null || tableVisitor.OnFileGroupOrPartitionScheme is null
            || tableVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count == 0)
            {
                return;
            }

            var detector = new LockEscalationVisitor(HandleNodeError);
            node.Accept(detector);
            if (!detector.Detected)
            {
                HandleNodeError(mainObjectDetector.ObjectDefinitionNode, "default is 'TABLE'");
            }
        }

        private class LockEscalationVisitor : VisitorWithCallback
        {
            public LockEscalationVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public bool Detected { get; private set; }

            public override void Visit(AlterTableSetStatement node)
            {
                var lockOption = node.Options.OfType<LockEscalationTableOption>().FirstOrDefault();
                if (lockOption != null)
                {
                    Detected = true;

                    if (lockOption.Value != LockEscalationMethod.Auto)
                    {
                        Callback(lockOption);
                    }
                }
            }
        }
    }
}
