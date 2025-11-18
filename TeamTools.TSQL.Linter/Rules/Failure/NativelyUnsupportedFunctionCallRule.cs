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
        private readonly BadFunctionDetector badFuncDetector;
        private readonly NativelyCompiledModuleDetector nativeCompilationDetector;

        public NativelyUnsupportedFunctionCallRule() : base()
        {
            badFuncDetector = new BadFunctionDetector(ViolationHandlerWithMessage);
            nativeCompilationDetector = new NativelyCompiledModuleDetector(DoValidateNativelyCompiledModule);
        }

        protected override void ValidateScript(TSqlScript node) => node.Accept(nativeCompilationDetector);

        private void DoValidateNativelyCompiledModule(TSqlFragment node, bool isProgrammability)
        {
            node?.Accept(badFuncDetector);
        }

        private sealed class BadFunctionDetector : TSqlFragmentVisitor
        {
            // TODO : consolidate all the metadata in resource file
            private static readonly HashSet<string> UnsupportedFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "APP_NAME",
                "ASCII",
                "BINARY_CHECKSUM",
                "CHAR",
                "CHARINDEX",
                "CHECKSUM",
                "COLUMNS_UPDATED",
                "COMPRESS",
                "CONNECTIONPROPERTY",
                "DATALENGTH",
                "DATENAME",
                "DATETIMEOFFSETFROMPARTS",
                "DB_ID",
                "DB_NAME",
                "DECOMPRESS",
                "DIFFERENCE",
                "FILE_ID",
                "FILE_NAME",
                "FORMAT",
                "FORMATMESSAGE",
                "HASHBYTES",
                "HOST_ID",
                "HOST_NAME",
                "IDENT_CURRENT",
                "IDENT_INCR",
                "IDENT_SEED",
                "ISDATE",
                "ISNUMERIC",
                "LEFT",
                "LOWER",
                "NCHAR",
                "OBJECT_ID",
                "OBJECT_NAME",
                "OBJECT_SCHEMA_NAME",
                "PARSE",
                "PATINDEX",
                "QUOTENAME",
                "REPLACE",
                "REPLICATE",
                "REVERSE",
                "RIGHT",
                "SCHEMA_ID",
                "SCHEMA_NAME",
                "SERVERPROPERTY",
                "SESSION_CONTEXT",
                "SOUNDEX",
                "SPACE",
                "SQL_VARIANT_PROPERTY",
                "STR",
                "STUFF",
                "SWITCHOFFSET",
                "SYSDATETIMEOFFSET",
                "TODATETIMEOFFSET",
                "TRY_CONVERT",
                "TRY_PARSE",
                "UNICODE",
                "UPPER",
                "XACT_STATE",
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
