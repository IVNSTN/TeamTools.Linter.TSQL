using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0180", "TEMP_TABLE_FILEGROUP")]
    internal sealed class TempTableFileGroupRule : AbstractRule
    {
        public TempTableFileGroupRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => ValidateFileGroup(node.SchemaObjectName, node.OnFileGroupOrPartitionScheme, () => node.Definition?.Accept(new IndexVisitor(HandleNodeError)));

        public override void Visit(CreateIndexStatement node) => ValidateFileGroup(node.OnName, node.OnFileGroupOrPartitionScheme);

        private void ValidateFileGroup(SchemaObjectName tableName, FileGroupOrPartitionScheme fg, Action next = default)
        {
            if (!tableName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            HandleNodeErrorIfAny(fg);

            next?.Invoke();
        }

        private class IndexVisitor : VisitorWithCallback
        {
            public IndexVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(IndexDefinition node) => DoValidate(node.OnFileGroupOrPartitionScheme);

            public override void Visit(UniqueConstraintDefinition node) => DoValidate(node.OnFileGroupOrPartitionScheme);

            private void DoValidate(FileGroupOrPartitionScheme fg)
            {
                if (fg != null)
                {
                    Callback(fg);
                }
            }
        }
    }
}
