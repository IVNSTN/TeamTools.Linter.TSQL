using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0198", "TRIGGER_CIRCULAR_ACTION")]
    [TriggerRule]
    internal sealed class TriggerCircularActionRule : AbstractRule
    {
        public TriggerCircularActionRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg);
            }
        }

        private void DoValidate(TriggerStatementBody node)
        {
            if (node.TriggerType == TriggerType.InsteadOf)
            {
                // ignoring instead-of triggers because they are not fired circularly
                return;
            }

            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // DDL triggers
                return;
            }

            string targetTable = node.TriggerObject.Name.GetFullName();
            List<TriggerActionType> triggerActions = new List<TriggerActionType>(node.TriggerActions.Select(a => a.TriggerActionType));

            var callback = new Action<string, TriggerActionType, TSqlFragment>((tblName, action, fragment) =>
            {
                // modifying the same table triggers is bound to
                if (tblName.Equals(targetTable, StringComparison.OrdinalIgnoreCase)
                && triggerActions.Contains(action))
                {
                    HandleNodeError(fragment);
                }
            });

            node.AcceptChildren(new TableModificationDetector(callback));
        }
    }
}
