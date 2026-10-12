using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
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

        private sealed class LockEscalationVisitor : TSqlFragmentVisitor
        {
            private static readonly string MsgNotForTempTable = Strings.ViolationDetails_RedundantLockEscalationSetRule_NotForTempTable;
            private static readonly string MsgThisIsDefault = Strings.ViolationDetails_RedundantLockEscalationSetRule_ThisIsDefault;
            private static readonly string MsgForPartitionedOnly = Strings.ViolationDetails_RedundantLockEscalationSetRule_ForPartitionedOnly;
            private static readonly string MsgDuplicate = Strings.ViolationDetails_RedundantLockEscalationSetRule_Duplicate;

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

                if (node.SchemaObjectName.IsTempTable())
                {
                    callback(lockOption, MsgNotForTempTable);
                    return;
                }

                if (lockOption.Value == LockEscalationMethod.Table)
                {
                    callback(lockOption, MsgThisIsDefault);
                }
                else if (!isPartitioned && lockOption.Value == LockEscalationMethod.Auto)
                {
                    callback(lockOption, MsgForPartitionedOnly);
                }

                if (lastSet != null)
                {
                    callback(lockOption, MsgDuplicate);
                }

                lastSet = lockOption;
            }

            private static LockEscalationTableOption DetectLockEscalation(IList<TableOption> options)
            {
                for (int i = options.Count - 1; i >= 0; i--)
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
