using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// UnPairedXmlDocumentInstructionRule implementation.
    /// </summary>
    [RuleIdentity("CS0921", "UNPAIRED_XMLDOC_STATEMENT")]
    internal sealed partial class UnPairedXmlDocumentInstructionRule : AbstractRule
    {
        public UnPairedXmlDocumentInstructionRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => ValidatePairedCalls(node);
    }
}
