using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class SpExecuteParameterExtractor
    {
        private static readonly string ProcName = "sp_executesql";
        private static readonly string ProcHeader = "CREATE PROCEDURE dbo.dummy ";
        private static readonly string AsFooter = "AS ;";
        // command + param definition + at least one param = 3
        private static readonly int MinParamsPassed = 3;

        public static IList<ProcedureParameter> ExtractDeclaredParameters(TSqlParser parser, ExecutableProcedureReference exec)
        {
            if (null != exec.ProcedureReference.ProcedureVariable)
            {
                return default;
            }

            if (exec.Parameters.Count < MinParamsPassed)
            {
                return default;
            }

            if (!exec.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.Equals(ProcName, StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }

            var paramDefinition = exec.Parameters[1];

            if (!(paramDefinition.ParameterValue is StringLiteral str))
            {
                // if params were defined in a variable
                // TODO : evaluate variable values
                return default;
            }

            return DoExtractDeclaredParameters(parser, str.Value);
        }

        private static IList<ProcedureParameter> DoExtractDeclaredParameters(TSqlParser parser, string declaration)
        {
            // TODO : try to parse declarations only without proc body surrounding
            var fragment = parser.Parse(new StringReader($"{ProcHeader} {declaration} {AsFooter}"), out IList<ParseError> err);
            if (err.Count > 0)
            {
                return null;
            }

            if (fragment is TSqlScript s && s.Batches.Count > 0)
            {
                var b = s.Batches[0];
                if (b.Statements.Count > 0 && b.Statements[0] is CreateProcedureStatement p)
                {
                    return p.Parameters;
                }
            }

            Debug.Fail("proc was not parsed correctly");

            return default;
        }
    }
}
