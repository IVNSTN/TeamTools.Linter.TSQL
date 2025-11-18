using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0837", "SP_XML_TO_XQUERY")]
    internal sealed class SpXmlToXQueryRule : AbstractRule
    {
        private static readonly string SpXmlPrepareProcedure = "sp_xml_preparedocument";

        public SpXmlToXQueryRule() : base()
        {
        }

        public override void Visit(OpenXmlTableReference node) => HandleNodeError(node);

        public override void Visit(ExecutableProcedureReference node)
        {
            var procRef = node.ProcedureReference.ProcedureReference?.Name;

            if (procRef is null
            || (procRef.SchemaIdentifier != null && procRef.SchemaIdentifier.Value.Equals(TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (procRef.BaseIdentifier.Value.Equals(SpXmlPrepareProcedure, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node);
            }
        }
    }
}
