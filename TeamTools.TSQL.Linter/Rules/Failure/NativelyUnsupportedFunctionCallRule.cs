using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0766", "NATIVELY_FUNCTION_NOT_SUPPORTED")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class NativelyUnsupportedFunctionCallRule : AbstractRule
    {
        public NativelyUnsupportedFunctionCallRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            NativelyCompiledModuleDetector.Detect(node, (nd, _) => DetectBadFunctions(nd));
        }

        private void DetectBadFunctions(TSqlFragment node)
        {
            node.Accept(new BadFunctionDetector(HandleNodeError));
        }

        private class BadFunctionDetector : TSqlFragmentVisitor
        {
            // TODO : consolidate all the metadata in resource file
            private static readonly ICollection<string> UnsupportedFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BINARY_CHECKSUM",
                "CHECKSUM",
                "DECOMPRESS",
                "COMPRESS",
                "HASHBYTES",
                "SESSION_CONTEXT",
                "XACT_STATE",
                "COLUMNS_UPDATED",
                "CONNECTIONPROPERTY",
                "SERVERPROPERTY",
                "HOST_NAME",
                "HOST_ID",
                "APP_NAME",
                "FILE_NAME",
                "FILE_ID",
                "DB_NAME",
                "DB_ID",
                "SCHEMA_ID",
                "SCHEMA_NAME",
                "OBJECT_SCHEMA_NAME",
                "OBJECT_NAME",
                "OBJECT_ID",
                "IDENT_INCR",
                "IDENT_CURRENT",
                "SQL_VARIANT_PROPERTY",
                "IDENT_SEED",
                "DATALENGTH",
                "TODATETIMEOFFSET",
                "SYSDATETIMEOFFSET",
                "SWITCHOFFSET",
                "DATETIMEOFFSETFROMPARTS",
                "DATENAME",
                "ISNUMERIC",
                "ISDATE",
                "TRY_PARSE",
                "PARSE",
                "TRY_CONVERT",
                "SOUNDEX",
                "DIFFERENCE",
                "PATINDEX",
                "CHARINDEX",
                "QUOTENAME",
                "FORMAT",
                "FORMATMESSAGE",
                "NCHAR",
                "CHAR",
                "UNICODE",
                "ASCII",
                "STR",
                "SPACE",
                "STUFF",
                "REVERSE",
                "REPLICATE",
                "REPLACE",
                "LEFT",
                "RIGHT",
                "UPPER",
                "LOWER",
            };

            private readonly Action<TSqlFragment, string> callback;

            public BadFunctionDetector(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(FunctionCall node) => ValidateFunction(node, node.FunctionName.Value);

            public override void Visit(LeftFunctionCall node) => ValidateFunction(node, "LEFT");

            public override void Visit(RightFunctionCall node) => ValidateFunction(node, "RIGHT");

            public override void Visit(TryConvertCall node) => ValidateFunction(node, "TRY_CONVERT");

            public override void Visit(TryParseCall node) => ValidateFunction(node, "TRY_PARSE");

            public override void Visit(ParseCall node) => ValidateFunction(node, "PARSE");

            private void ValidateFunction(TSqlFragment node, string functionName)
            {
                if (!string.IsNullOrEmpty(functionName) && UnsupportedFunctions.Contains(functionName))
                {
                    callback(node, functionName);
                }
            }
        }
    }
}
