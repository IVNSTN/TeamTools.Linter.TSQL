using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0779", "EXECUTE_AS_SELF")]
    internal sealed class ExecuteAsSelfRule : AbstractRule
    {
        public ExecuteAsSelfRule() : base()
        {
        }

        public override void Visit(ExecuteAsClause node)
        {
            if (node.ExecuteAsOption == ExecuteAsOption.Self)
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(CreateQueueStatement node)
        {
            // If option was provided then it is caught by the other visitor method
            // FIXME : actually this is not really possible syntax
            if (!node.QueueOptions.HasOption(QueueOptionKind.ActivationExecuteAs)
            && node.QueueOptions.HasOption(QueueOptionKind.ActivationProcedureName))
            {
                // omitted == SELF
                HandleNodeErrorIfAny(node);
            }
        }
    }
}
