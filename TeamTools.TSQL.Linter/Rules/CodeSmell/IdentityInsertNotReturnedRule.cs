using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0462", "IDENTITY_INSERT_NOT_RETURNED")]
    internal sealed partial class IdentityInsertNotReturnedRule : AbstractRule
    {
        public IdentityInsertNotReturnedRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            var tracker = new IdentityInsertTracker();
            batch.AcceptChildren(tracker);

            foreach (var node in tracker.OpenStatements)
            {
                HandleNodeError(node);
            }
        }

        private class IdentityInsertTracker : TSqlFragmentVisitor
        {
            private readonly Dictionary<string, TSqlFragment> openStatements =
                new Dictionary<string, TSqlFragment>(System.StringComparer.OrdinalIgnoreCase);

            public IEnumerable<TSqlFragment> OpenStatements => openStatements.Values;

            public override void Visit(SetIdentityInsertStatement node)
            {
                string tableName = node.Table.GetFullName();

                if (tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    // SQL Server does not allow SET IDENTITY_INSERT on temp tables and table variables
                    // on these object types (raises error 4108). There is a separate rule to detect such
                    // syntax violation.
                    return;
                }

                if (node.IsOn)
                {
                    openStatements[tableName] = node;
                }
                else
                {
                    openStatements.Remove(tableName);
                }
            }
        }
    }
}
