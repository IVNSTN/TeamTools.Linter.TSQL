using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0857", "SINGLE_OBJECT_PER_FILE")]
    internal sealed partial class SingleObjectPerFileRule : AbstractRule
    {
        public SingleObjectPerFileRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node) => node.AcceptChildren(new CreateVisitor(ViolationHandler));
    }
}
