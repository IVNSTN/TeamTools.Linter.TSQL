using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class RedundantTableCleanupRule
    {
        private sealed class TableCleanupVisitor : TSqlFragmentVisitor
        {
            public TSqlFragment ViolationNode { get; private set; }

            // Inside loop there may be reason for cleanup before next stage
            public override void ExplicitVisit(WhileStatement node)
            { }

            // These statements could be affected by temp table cleanup
            public override void Visit(DataModificationStatement node) => Reset();

            public override void Visit(ExecuteStatement node) => Reset();

            // Temp table content cleanup
            public override void Visit(TruncateTableStatement node)
            {
                if (IsTempTable(node.TableName))
                {
                    ViolationNode = node;
                }
            }

            // Temp table removal
            public override void Visit(DropTableStatement node)
            {
                for (int i = node.Objects.Count - 1; i >= 0; i--)
                {
                    if (node.Objects[i] is SchemaObjectName schemaName && IsTempTable(schemaName))
                    {
                        ViolationNode = node;
                        return;
                    }
                }
            }

            private static bool IsTempTable(SchemaObjectName schemaName) => schemaName.IsTempTable();

            private void Reset() => ViolationNode = null;
        }
    }
}
