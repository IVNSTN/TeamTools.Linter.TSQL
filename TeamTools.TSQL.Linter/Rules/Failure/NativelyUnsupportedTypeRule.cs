using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0760", "NATIVELY_UNSUPPORTED_TYPE")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class NativelyUnsupportedTypeRule : AbstractRule
    {
        public NativelyUnsupportedTypeRule() : base()
        {
        }

        public override void Visit(TSqlStatement node)
        {
            NativelyCompiledModuleDetector.Detect(node, DetectBadTypes);
        }

        private void DetectBadTypes(TSqlFragment node, bool isProgrammability)
        {
            node.AcceptChildren(new BadTypeDetector(HandleNodeError, !isProgrammability));
        }

        private class BadTypeDetector : TSqlFragmentVisitor
        {
            private static readonly ICollection<string> UnsupportedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DATETIMEOFFSET",
                "ROWVERSION",
                "TIMESTAMP",
                "HIERARCHYID",
                "SQL_VARIANT",
                "XML",
                "GEOGRAPHY",
                "GEOMETRY",
            };

            private readonly Action<TSqlFragment, string> callback;
            private readonly bool reportOnUdfs;

            public BadTypeDetector(Action<TSqlFragment, string> callback, bool reportOnUdfs)
            {
                this.callback = callback;
                this.reportOnUdfs = reportOnUdfs;
            }

            public override void Visit(DataTypeReference node)
            {
                if (UnsupportedTypes.Contains(node.Name.BaseIdentifier.Value))
                {
                    // GEOGRAPHY is parsed as UDT
                    // thus catching SqlDataTypeReference is not enough
                    callback(node, node.Name.BaseIdentifier.Value.ToUpperInvariant());
                }
                else if (reportOnUdfs && !(node is SqlDataTypeReference))
                {
                    callback(node, node.Name.GetFullName());
                }
            }
        }
    }
}
