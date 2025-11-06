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

        public override void Visit(TriggerStatementBody node)
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
            List<TriggerActionType> triggerActions = new List<TriggerActionType>();
            triggerActions.AddRange(node.TriggerActions.Select(a => a.TriggerActionType).Distinct<TriggerActionType>());

            var modifiedTables = new SortedDictionary<string, IList<KeyValuePair<TriggerActionType, TSqlFragment>>>(StringComparer.OrdinalIgnoreCase);
            var modifications = new TableModificationDetector((tblName, action, fragment) =>
            {
                if (!modifiedTables.ContainsKey(tblName))
                {
                    modifiedTables.Add(tblName, new List<KeyValuePair<TriggerActionType, TSqlFragment>>());
                }

                if (!modifiedTables[tblName].Any(p => p.Key == action))
                {
                    modifiedTables[tblName].Add(new KeyValuePair<TriggerActionType, TSqlFragment>(action, fragment));
                }
            });
            node.AcceptChildren(modifications);

            // same table
            if (!modifiedTables.ContainsKey(targetTable))
            {
                return;
            }

            // modification matches trigger handled event
            var issue = modifiedTables[targetTable].FirstOrDefault(act => triggerActions.Contains(act.Key)).Value;
            HandleNodeErrorIfAny(issue);
        }
    }
}
