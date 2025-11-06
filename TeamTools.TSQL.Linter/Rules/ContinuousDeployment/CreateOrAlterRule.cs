using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0717", "CREATE_OR_ALTER")]
    internal sealed class CreateOrAlterRule : AbstractRule
    {
        public CreateOrAlterRule() : base()
        {
        }

        public override void Visit(CreateOrAlterFunctionStatement node) => HandleNodeError(node);

        public override void Visit(CreateOrAlterProcedureStatement node) => HandleNodeError(node);

        public override void Visit(CreateOrAlterTriggerStatement node) => HandleNodeError(node);

        public override void Visit(CreateOrAlterViewStatement node) => HandleNodeError(node);
    }
}
