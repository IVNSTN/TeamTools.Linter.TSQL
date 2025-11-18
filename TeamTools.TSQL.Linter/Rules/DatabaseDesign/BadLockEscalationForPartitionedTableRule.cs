using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Rule implementation is very similar to RedundantLockEscalationSetRule
    [RuleIdentity("DD0759", "PARTITIONED_TABLE_LOCK_ESCALATION")]
    internal sealed class BadLockEscalationForPartitionedTableRule : ScriptAnalysisServiceConsumingRule
    {
        public BadLockEscalationForPartitionedTableRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var mainObject = GetService<MainScriptObjectDetector>(node);

            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName)
            || !(mainObject.ObjectDefinitionNode is CreateTableStatement createTable)
            || mainObject.ObjectFullName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // non CREATE-TABLE script
                return;
            }

            var tableVisitor = GetService<TableIndicesVisitor>(node);

            if (tableVisitor.Table is null
            || tableVisitor.OnFileGroupOrPartitionScheme is null
            || tableVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count == 0)
            {
                return;
            }

            var detector = new LockEscalationVisitor(ViolationHandler);
            node.Accept(detector);
            if (!detector.Detected)
            {
                HandleNodeError(tableVisitor.OnFileGroupOrPartitionScheme, Strings.ViolationDetails_BadLockEscalationForPartitionedTableRule_TableIsDefault);
            }
        }

        private sealed class LockEscalationVisitor : VisitorWithCallback
        {
            public LockEscalationVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public bool Detected { get; private set; }

            public override void Visit(AlterTableSetStatement node)
            {
                var lockOption = DetectLockEscalation(node.Options);
                if (lockOption != null)
                {
                    Detected = true;

                    if (lockOption.Value != LockEscalationMethod.Auto)
                    {
                        Callback(lockOption);
                    }
                }
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
