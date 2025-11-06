using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0906", "FK_RECURSION")]
    internal sealed class ForeignKeyRecursionRule : AbstractRule
    {
        public ForeignKeyRecursionRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            string srcTable = node.SchemaObjectName.GetFullName();

            var fkVisitor = new RecursiveForeignKeyVisitor(srcTable, (fk) => HandleNodeError(node, fk));
            node.AcceptChildren(fkVisitor);
        }

        public override void Visit(AlterTableStatement node)
        {
            string srcTable = node.SchemaObjectName.GetFullName();

            var fkVisitor = new RecursiveForeignKeyVisitor(srcTable, (fk) => HandleNodeError(node, fk));
            node.AcceptChildren(fkVisitor);
        }

        private class RecursiveForeignKeyVisitor : TSqlFragmentVisitor
        {
            private readonly string srcTable;
            private readonly Action<string> callback;

            public RecursiveForeignKeyVisitor(string srcTable, Action<string> callback)
            {
                this.srcTable = srcTable;
                this.callback = callback;
            }

            public override void Visit(ForeignKeyConstraintDefinition node)
            {
                string dstTable = node.ReferenceTableName.GetFullName();

                if (string.Equals(dstTable, srcTable, StringComparison.OrdinalIgnoreCase))
                {
                    callback(node.ConstraintIdentifier.Value);
                }
            }
        }
    }
}
