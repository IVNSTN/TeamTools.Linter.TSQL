using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

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
            if (!node.QueueOptions.Any(opt => opt.OptionKind == QueueOptionKind.ActivationExecuteAs)
            && node.QueueOptions.Any(opt => opt.OptionKind == QueueOptionKind.ActivationProcedureName))
            {
                // omitted == SELF
                HandleNodeErrorIfAny(node);
            }
        }
    }
}
