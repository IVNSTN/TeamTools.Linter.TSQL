using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0760", "NATIVELY_UNSUPPORTED_TYPE")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class NativelyUnsupportedTypeRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private readonly NativelyCompiledModuleDetector nativeCompilationDetector;
        private BadTypeDetector badTypesWithoutUdt;
        private BadTypeDetector badTypesWithUdt;
        private HashSet<string> unsupportedTypes;
        private HashSet<string> systemTypes;

        public NativelyUnsupportedTypeRule() : base()
        {
            nativeCompilationDetector = new NativelyCompiledModuleDetector(DoValidateNativelyCompiledModule);
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            unsupportedTypes = new HashSet<string>(
                data.Types
                    .Where(t => !t.Value.CanBeNativelyCompiled)
                    .Select(t => t.Key),
                StringComparer.OrdinalIgnoreCase);

            systemTypes = new HashSet<string>(data.Types.Keys, StringComparer.OrdinalIgnoreCase);

            badTypesWithUdt = new BadTypeDetector(unsupportedTypes, systemTypes, true, ViolationHandlerWithMessage);
            badTypesWithoutUdt = new BadTypeDetector(unsupportedTypes, systemTypes, false, ViolationHandlerWithMessage);
        }

        protected override void ValidateScript(TSqlScript node) => node.Accept(nativeCompilationDetector);

        private void DoValidateNativelyCompiledModule(TSqlFragment node, bool isProgrammability)
        {
            Debug.Assert(systemTypes != null && systemTypes.Count > 0, "systemTypes not loaded");

            if (isProgrammability)
            {
                node?.AcceptChildren(badTypesWithoutUdt);
            }
            else
            {
                node?.AcceptChildren(badTypesWithUdt);
            }
        }

        private sealed class BadTypeDetector : TSqlFragmentVisitor
        {
            private readonly HashSet<string> unsupportedTypes;
            private readonly HashSet<string> systemTypes;
            private readonly Action<TSqlFragment, string> callback;
            private readonly bool reportOnUdts;

            public BadTypeDetector(
                HashSet<string> unsupportedTypes,
                HashSet<string> systemTypes,
                bool reportOnUdts,
                Action<TSqlFragment, string> callback)
            {
                this.unsupportedTypes = unsupportedTypes;
                this.systemTypes = systemTypes;
                this.reportOnUdts = reportOnUdts;
                this.callback = callback;
            }

            public override void Visit(DataTypeReference node)
            {
                string typeName = node.GetFullName();

                if (string.IsNullOrEmpty(typeName))
                {
                    return;
                }

                if (unsupportedTypes.Contains(typeName))
                {
                    // unsupported system type
                    callback(node, typeName);
                }
                else if (reportOnUdts && !(node is SqlDataTypeReference)
                // some types like GEOGRAPHY or JSON are parsed as UDT reference
                && !systemTypes.Contains(typeName))
                {
                    // user defined type
                    callback(node, typeName);
                }
            }
        }
    }
}
