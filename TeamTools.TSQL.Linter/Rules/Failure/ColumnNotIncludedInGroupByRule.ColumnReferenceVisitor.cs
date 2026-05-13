using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ColumnNotIncludedInGroupByRule
    {
        private sealed class ColumnReferenceVisitor : TSqlViolationDetector
        {
            private readonly HashSet<string> nonColumnIdentifiers;

            public ColumnReferenceVisitor(HashSet<string> nonColumnIdentifiers)
            {
                this.nonColumnIdentifiers = nonColumnIdentifiers;
            }

            public void Reset() => Detected = false;

            public override void Visit(ColumnReferenceExpression node)
            {
                if (node.ColumnType == ColumnType.Wildcard)
                {
                    return;
                }

                if (node.MultiPartIdentifier.Identifiers.Count == 1
                && nonColumnIdentifiers.Contains(node.MultiPartIdentifier.Identifiers[0].Value))
                {
                    return;
                }

                MarkDetected(node);
            }
        }
    }
}
