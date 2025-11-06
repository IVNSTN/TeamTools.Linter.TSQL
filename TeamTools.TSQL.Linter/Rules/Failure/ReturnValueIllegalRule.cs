using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0119", "RETURN_VALUE_ILLEGAL")]
    internal sealed class ReturnValueIllegalRule : AbstractRule
    {
        public ReturnValueIllegalRule() : base()
        {
        }

        public override void Visit(CreateTriggerStatement node) => DetectIllegalValue(node);

        public override void Visit(CreateFunctionStatement node)
        {
            if (node.ReturnType is ScalarFunctionReturnType)
            {
                return;
            }

            DetectIllegalValue(node);
        }

        private void DetectIllegalValue(TSqlFragment node)
        {
            var returnVisitor = new ReturnValueVisitor();
            node.AcceptChildren(returnVisitor);
            HandleNodeErrorIfAny(returnVisitor.FirstDetectedNode);
        }

        private class ReturnValueVisitor : TSqlViolationDetector
        {
            public override void Visit(ReturnStatement node)
            {
                if (node.Expression != null)
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
