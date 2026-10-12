using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal sealed partial class ScopeIdentityAfterMultiRowDmlRule
    {
        private sealed class BatchScopeIdentityDetector : VisitorWithCallback
        {
            private static readonly string ScopeIdentityFunctionName = "SCOPE_IDENTITY";

            private TSqlFragment multiRowDmlLocation = null;

            public BatchScopeIdentityDetector(Action<TSqlFragment> callback) : base(callback)
            {
            }

            public override void Visit(FunctionCall node)
            {
                if (multiRowDmlLocation is null)
                {
                    // No multi-row DML detected before
                    return;
                }

                if (node.FunctionName.Value.Equals(ScopeIdentityFunctionName, StringComparison.OrdinalIgnoreCase))
                {
                    Callback(node);
                }
            }

            // Detecting potentially multi-row DML
            public override void ExplicitVisit(InsertSpecification node)
            {
                multiRowDmlLocation = MultiRowDmlDetector.IsMultiRowDml(node) ? node : null;
            }

            public override void ExplicitVisit(MergeSpecification node)
            {
                multiRowDmlLocation = MultiRowDmlDetector.IsMultiRowDml(node) ? node : null;
            }
        }
    }
}
