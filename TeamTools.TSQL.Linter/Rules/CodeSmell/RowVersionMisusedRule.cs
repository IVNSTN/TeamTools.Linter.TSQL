using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0787", "ROWVERSION_MISUSED")]
    internal sealed class RowVersionMisusedRule : AbstractRule
    {
        private readonly RowVersionDetector rowVersionDetector;

        public RowVersionMisusedRule() : base()
        {
            rowVersionDetector = new RowVersionDetector(ViolationHandler);
        }

        public override void Visit(CreateTypeTableStatement node) => DetectRowVersionUsage(node.Definition);

        public override void Visit(DeclareTableVariableBody node) => DetectRowVersionUsage(node.Definition);

        public override void Visit(CreateTableStatement node) => ValidateTableColumns(node.SchemaObjectName, node.Definition);

        public override void Visit(AlterTableAddTableElementStatement node) => ValidateTableColumns(node.SchemaObjectName, node.Definition);

        public override void Visit(AlterTableAlterColumnStatement node) => ValidateTableColumns(node.SchemaObjectName, node.DataType);

        private void ValidateTableColumns(SchemaObjectName name, TSqlFragment definition)
        {
            if (!name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // we need temp tables only
                return;
            }

            DetectRowVersionUsage(definition);
        }

        private void DetectRowVersionUsage(TSqlFragment node)
        {
            if (node is null)
            {
                return;
            }

            node.Accept(rowVersionDetector);
        }

        private class RowVersionDetector : VisitorWithCallback
        {
            private static readonly HashSet<string> TypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ROWVERSION",
                "TIMESTAMP",
            };

            public RowVersionDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DataTypeReference typeref)
            {
                if (typeref.Name is null)
                {
                    // e.g. CURSOR
                    return;
                }

                if (TypeNames.Contains(typeref.Name.BaseIdentifier.Value))
                {
                    Callback(typeref);
                }
            }
        }
    }
}
