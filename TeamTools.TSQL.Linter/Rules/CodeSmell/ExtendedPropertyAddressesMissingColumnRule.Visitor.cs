using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExtendedPropertyAddressesMissingColumnRule
    {
        private sealed class ExtendedPropertyVisitor : ExtendedPropertyEditingVisitor
        {
            private readonly Dictionary<string, string> argValueMap;
            private readonly IDictionary<string, SqlColumnInfo> validColumns;

            public ExtendedPropertyVisitor(
                SchemaObjectName expectedTarget,
                IDictionary<string, SqlColumnInfo> validColumns,
                Action<TSqlFragment, string> callback) : base(callback)
            {
                this.validColumns = validColumns ?? throw new ArgumentNullException(nameof(validColumns));

                string expectedObjectName = expectedTarget.BaseIdentifier.Value ?? throw new ArgumentNullException(nameof(expectedTarget));
                string expectedSchemaName = expectedTarget.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                argValueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "@level0type", "SCHEMA" },
                    { "@level0name", expectedSchemaName },
                    { "@level1type", "TABLE" },
                    { "@level1name", expectedObjectName },
                    { "@level2type", "COLUMN" },
                    { "@level2name", "<col name>" },
                };
            }

            protected override void ValidatePropertyEditingProcArgs(IList<ExecuteParameter> procParams, TSqlFragment call)
            {
                StringLiteral columnMentionNode = null;

                int argCollectionFlags = MatchArgs(
                    argValueMap,
                    procParams,
                    (param, expectedArgValue) =>
                    {
                        if (string.Equals(expectedArgValue, "<col name>")
                        && param.ParameterValue is StringLiteral levelTypeValue)
                        {
                            columnMentionNode = levelTypeValue;
                        }
                    });

                // 32 - SCHEMA and TABLE names matched, COLUMN name provided
                if (argCollectionFlags == 32
                && columnMentionNode != null
                && !validColumns.ContainsKey(columnMentionNode.Value))
                {
                    Callback(columnMentionNode, columnMentionNode.Value);
                }
            }
        }
    }
}
