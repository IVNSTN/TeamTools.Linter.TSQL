using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0753", "DROP_STATEMENTS_INTO_ONE")]
    internal sealed class DropStatementsIntoOneRule : AbstractRule
    {
        public DropStatementsIntoOneRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new ConsecutiveDropsDetector(ViolationHandler));

        private class ConsecutiveDropsDetector : VisitorWithCallback
        {
            private static readonly int MaxDropPerStatement = 12;
            private string lastDroppedObjectType;
            private int lastDroppedElementCount;

            public ConsecutiveDropsDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DropAggregateStatement node) => Drop("AGGREGATE", node);

            public override void Visit(DropAssemblyStatement node) => Drop("ASSEMBLY", node);

            public override void Visit(DropDefaultStatement node) => Drop("DEFAULT", node);

            public override void Visit(DropExternalTableStatement node) => Drop("EXTTBL", node);

            public override void Visit(DropFunctionStatement node) => Drop("FUNC", node);

            public override void Visit(DropProcedureStatement node) => Drop("PROC", node);

            public override void Visit(DropRuleStatement node) => Drop("RULE", node);

            public override void Visit(DropSecurityPolicyStatement node) => Drop("SECPOL", node);

            public override void Visit(DropSequenceStatement node) => Drop("SEQUENCE", node);

            public override void Visit(DropSynonymStatement node) => Drop("SYNONYM", node);

            public override void Visit(DropTableStatement node) => Drop("TABLE", node);

            public override void Visit(DropTriggerStatement node) => Drop("TRIGGER", node);

            public override void Visit(DropViewStatement node) => Drop("VIEW", node);

            public override void Visit(TSqlStatement node)
            {
                // DropObjectsStatement descendants are handled by other methods
                // declarations and set-var don't seem to be logic breakers
                if (node is DropObjectsStatement
                || node is SetVariableStatement
                || node is DeclareVariableStatement
                || node is DeclareTableVariableStatement)
                {
                    return;
                }

                ResetLastCmd();
            }

            private void ResetLastCmd() => lastDroppedObjectType = "";

            private void Drop(string objectType, DropObjectsStatement node)
            {
                if (string.Equals(lastDroppedObjectType, objectType, StringComparison.OrdinalIgnoreCase)
                && (node.Objects.Count + lastDroppedElementCount) < MaxDropPerStatement)
                {
                    Callback(node);
                }

                lastDroppedObjectType = objectType;
                lastDroppedElementCount = node.Objects.Count;
            }
        }
    }
}
