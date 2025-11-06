using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0291", "INDEX_CLUSTERED_OR_NOT")]
    [IndexRule]
    internal sealed class IndexClusteredAmbiguityRule : AbstractRule
    {
        private readonly TableDefinitionVisitor tblVisitor;

        public IndexClusteredAmbiguityRule() : base()
        {
            tblVisitor = new TableDefinitionVisitor(HandleNodeError);
        }

        public override void Visit(CreateTableStatement node)
        {
            string tableName = node.SchemaObjectName.BaseIdentifier.Value;
            if (tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // ignoring temp table and table variable definitions
                return;
            }

            node.AcceptChildren(tblVisitor);
        }

        public override void Visit(CreateIndexStatement node)
        {
            if (node.Clustered != null)
            {
                return;
            }

            HandleNodeError(node);
        }

        private class TableDefinitionVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment> callback;

            public TableDefinitionVisitor(Action<TSqlFragment> callback)
            {
                this.callback = callback;
            }

            public override void Visit(UniqueConstraintDefinition node)
            {
                if (node.Clustered != null)
                {
                    return;
                }

                // they are all specifically clustered/nonclustered
                if (node.IndexType?.IndexTypeKind != null)
                {
                    return;
                }

                callback(node);
            }

            public override void Visit(IndexDefinition node)
            {
                if (node.IndexType?.IndexTypeKind != null)
                {
                    return;
                }

                callback(node);
            }
        }
    }
}
