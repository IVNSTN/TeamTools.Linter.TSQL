using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// UnPairedTransactionInstructionRule implementation.
    /// </summary>
    [RuleIdentity("CS0920", "UNPAIRED_TRAN_STATEMENT")]
    internal sealed partial class UnPairedTransactionInstructionRule : AbstractRule
    {
        private static readonly string NoNameTranId = "<noname>";

        public UnPairedTransactionInstructionRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var callVisitor = new TranVisitor();
            node.AcceptChildren(callVisitor);

            if (callVisitor.Trans.Count == 0)
            {
                return;
            }

            AnalyzeTranStatements(callVisitor.Trans);
        }

        private void ReportError(TSqlFragment node, string tranName)
        {
            HandleNodeError(node, tranName.Equals(NoNameTranId) ? default : tranName);
        }
    }
}
