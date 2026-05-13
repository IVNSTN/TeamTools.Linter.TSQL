using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExtendedPropertyMisdirectedRule
    {
        private sealed class ExtendedPropertyVisitor : ExtendedPropertyEditingVisitor
        {
            private readonly Dictionary<string, string> argValueMap;

            public ExtendedPropertyVisitor(
                SchemaObjectName expectedTarget,
                string targetObjectType,
                Action<TSqlFragment, string> callback) : base(callback)
            {
                if (string.IsNullOrEmpty(targetObjectType))
                {
                    throw new ArgumentNullException(nameof(targetObjectType));
                }

                string expectedObjectName = expectedTarget.BaseIdentifier.Value ?? throw new ArgumentNullException(nameof(expectedTarget));
                string expectedSchemaName = expectedTarget.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                argValueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "@level0type", "SCHEMA" },
                    { "@level0name", expectedSchemaName },
                    { "@level1type", targetObjectType },
                    { "@level1name", expectedObjectName },
                };
            }

            protected override void ValidatePropertyEditingProcArgs(IList<ExecuteParameter> procParams, TSqlFragment call)
            {
                TSqlFragment lastMismatch = null;

                int argCollectionFlags = MatchArgs(
                    argValueMap,
                    procParams,
                    (param, _) => lastMismatch = param);

                // argCollectionFlags >= 20 - both SCHEMA and object type (e.g. TABLE) level filters were provided
                // argCollectionFlags != 22 - either schema or object name has wrong value
                // argCollectionFlags  < 20 - extended property points to something completely unexpected
                if (argCollectionFlags < 22)
                {
                    // If lastMismatch is null then the whole call is either broken
                    // or relates to something completely unexpected
                    Callback(lastMismatch ?? call, default);
                }
            }
        }
    }
}
