using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0120", "BODY_BEGIN_END")]
    internal sealed class BeginEndForBodyRule : AbstractRule
    {
        public BeginEndForBodyRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node) => DoValidateBody(node.StatementList);

        public override void Visit(TriggerStatementBody node) => DoValidateBody(node.StatementList);

        private void DoValidateBody(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // CLR / external
                return;
            }

            if (body.Statements[0] is BeginEndBlockStatement)
            {
                return;
            }

            HandleNodeError(body.Statements[0]);
        }
    }
}
