using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0131", "SET_OPTION_ILLEGAL")]
    internal sealed class SetOptionInFunctionRule : AbstractRule
    {
        public SetOptionInFunctionRule() : base()
        {
        }

        public override void Visit(CreateFunctionStatement node)
        {
            var opt = new SetOptionVisitor();
            node.AcceptChildren(opt);

            if (opt.HasSetOptionStatement == false)
            {
                return;
            }

            HandleNodeError(opt.SetStatement);
        }

        private class SetOptionVisitor : TSqlFragmentVisitor
        {
            public bool HasSetOptionStatement
            { get; private set; } = false;

            public PredicateSetStatement SetStatement { get; private set; }

            public override void Visit(PredicateSetStatement node)
            {
                HasSetOptionStatement = true;
                SetStatement = node;
            }
        }
    }
}
