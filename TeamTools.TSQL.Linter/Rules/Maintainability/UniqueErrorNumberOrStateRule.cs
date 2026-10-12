using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0148", "UNIQUE_ERR_NO_OR_STATE")]

    internal sealed partial class UniqueErrorNumberOrStateRule : AbstractRule
    {
        public UniqueErrorNumberOrStateRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
            => node.AcceptChildren(new ErrorVisitor(ViolationHandler));
    }
}
