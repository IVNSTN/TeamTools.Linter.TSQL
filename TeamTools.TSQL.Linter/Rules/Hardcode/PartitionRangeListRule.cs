using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0121", "PARTITIONS_HARDCODED")]
    internal sealed class PartitionRangeListRule : AbstractRule
    {
        public PartitionRangeListRule() : base()
        {
        }

        public override void Visit(AlterTableRebuildStatement node) => ValidateCompressionOptions(node.IndexOptions);

        public override void Visit(AlterTableAlterIndexStatement node) => ValidateCompressionOptions(node.IndexOptions);

        public override void Visit(CreateTableStatement node) => ValidateCompressionOptions(node.Options);

        public override void Visit(UniqueConstraintDefinition node) => ValidateCompressionOptions(node.IndexOptions);

        public override void Visit(CreateColumnStoreIndexStatement node) => ValidateCompressionOptions(node.IndexOptions);

        public override void Visit(IndexDefinition node) => ValidateCompressionOptions(node.IndexOptions);

        public override void Visit(IndexStatement node) => ValidateCompressionOptions(node.IndexOptions);

        private static void DetectHardcodedPartitions<T>(IList<T> options, Action<TSqlFragment> callback)
        where T : TSqlFragment
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = options[i];
                if (opt is DataCompressionOption dcmp)
                {
                    // dummy
                }
                else if (opt is TableDataCompressionOption cmp)
                {
                    dcmp = cmp.DataCompressionOption;
                }
                else
                {
                    continue;
                }

                if (dcmp != null && dcmp.PartitionRanges.Count > 0)
                {
                    callback.Invoke(dcmp);
                    // one violation per object is enough
                    return;
                }
            }
        }

        private void ValidateCompressionOptions(IList<IndexOption> options) => DetectHardcodedPartitions(options, ViolationHandler);

        private void ValidateCompressionOptions(IList<TableOption> options) => DetectHardcodedPartitions(options, ViolationHandler);
    }
}
