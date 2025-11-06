using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
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

        private void ValidateCompressionOptions(IList<IndexOption> options)
        {
            var optionsWithPartitions = options
                .OfType<DataCompressionOption>()
                .FirstOrDefault(opt => opt.PartitionRanges.Count > 0);

            HandleNodeErrorIfAny(optionsWithPartitions);
        }

        private void ValidateCompressionOptions(IList<TableOption> options)
        {
            var optionsWithPartitions = options
                .OfType<TableDataCompressionOption>()
                .FirstOrDefault(opt => opt.DataCompressionOption.PartitionRanges.Count > 0);

            HandleNodeErrorIfAny(optionsWithPartitions);
        }
    }
}
