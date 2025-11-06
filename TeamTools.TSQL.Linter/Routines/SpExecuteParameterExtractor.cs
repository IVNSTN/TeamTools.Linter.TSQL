using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SpExecuteParameterExtractor
    {
        private static readonly string ProcName = "sp_executesql";

        public static IList<ProcedureParameter> ExtractDeclaredParameters(TSqlParser parser, ExecutableProcedureReference exec)
        {
            if (null != exec.ProcedureReference.ProcedureVariable)
            {
                return default;
            }

            if (!exec.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.Equals(ProcName, StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }

            // command + param definition + at least one param = 3
            if (exec.Parameters.Count < 3)
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
            declaration = string.Concat(
                "CREATE PROCEDURE dbo.dummy ",
                declaration,
                " AS ;");

            var fragment = parser.Parse(new StringReader(declaration), out IList<ParseError> err);
            if (err.Count > 0)
            {
                return null;
            }

            var paramVisitor = new ProcParamVisitor();
            fragment.Accept(paramVisitor);

            return paramVisitor.Parameters;
        }

        private class ProcParamVisitor : TSqlFragmentVisitor
        {
            public IList<ProcedureParameter> Parameters { get; private set; }

            public override void Visit(ProcedureStatementBody node)
            {
                Parameters = node.Parameters;
            }
        }
    }
}
